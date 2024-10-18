using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCryptNext = BCrypt.Net.BCrypt;

namespace Faluf.Trading.Infrastructure.Services;

public sealed class AuthService(IUserRepository userRepository, IAuthStateRepository authStateRepository, ILogger<AuthService> logger, IStringLocalizer<AuthService> stringLocalizer, IConfiguration configuration) 
	: IAuthService
{
	private readonly string secret = configuration["JWT:Secret"]!;
	private readonly string issuer = configuration["JWT:Issuer"]!;
	private readonly string audience = configuration["JWT:Audience"]!;
	private readonly int accessTokenExpiryInMinutes = int.Parse(configuration["JWT:AccessTokenExpiryInMinutes"]!);
	private readonly int refreshTokenExpiryInDays = int.Parse(configuration["JWT:RefreshTokenExpiryInDays"]!);

	public async Task<Result<TokenDTO>> LoginAsync(LoginInputModel loginInputModel, CancellationToken cancellationToken = default)
	{
		try
		{
			User? user = await userRepository.GetByEmailAsync(loginInputModel.Email, cancellationToken).ConfigureAwait(false);

			if (user is null)
			{
				logger.LogInformation("User not found by email.");

				return Result.Unauthorized<TokenDTO>(stringLocalizer["BadCredentials"]);
			}

			AuthState? authState = await authStateRepository.GetByUserIdAndClientTypeAsync(user.Id, loginInputModel.ClientType, cancellationToken);
			authState ??= new() { UserId = user.Id, ClientType = loginInputModel.ClientType };

			if (authState.LockoutEndUTC > DateTimeOffset.UtcNow)
			{
				logger.LogInformation("User is locked out.");

				TimeSpan lockoutEnd = (authState.LockoutEndUTC - DateTimeOffset.UtcNow).Value;
				double lockoutEndMinutes = Math.Ceiling(lockoutEnd.TotalMinutes);
				double lockoutEndSeconds = Math.Ceiling(lockoutEnd.TotalSeconds);

				return Result.Locked<TokenDTO>(stringLocalizer["LockoutMessage", lockoutEndMinutes, lockoutEndSeconds]);
			}

			bool isValidPassword = BCryptNext.Verify(loginInputModel.Password, user.HashedPassword);

			if (!isValidPassword)
			{
				logger.LogInformation("Bad credentials.");

				if (++authState.AccessFailedCount >= 5)
				{
					logger.LogInformation("User is now locked out.");

					authState.LockoutEndUTC = DateTime.UtcNow.AddMinutes(5);
					authState.AccessFailedCount = 0;
				}

				await authStateRepository.UpsertAsync(authState, cancellationToken).ConfigureAwait(false);

				return Result.Unauthorized<TokenDTO>(stringLocalizer["BadCredentials", authState.AccessFailedCount]);
			}

			authState.AccessFailedCount = 0;
			authState.LockoutEndUTC = null;
			authState.RefreshToken = Guid.NewGuid().ToString();
			authState.RefreshTokentExpiryUTC = DateTimeOffset.UtcNow.AddDays(refreshTokenExpiryInDays);

			await authStateRepository.UpsertAsync(authState, cancellationToken).ConfigureAwait(false);

			List<Claim> claims =
			[
				new(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new(ClaimTypes.Name, user.FirstName),
				new(ClaimTypes.Surname, user.LastName),
				new(ClaimTypes.Email, user.Email),
				new(JwtRegisteredClaimNames.Jti, authState.RefreshToken)
			];

			user.Roles.ForEach(role => claims.Add(new(ClaimTypes.Role, role)));

			string accessToken = GenerateAccessToken(claims);

			return Result.Ok(new TokenDTO(accessToken, authState.RefreshToken));
		}
		catch (Exception ex)
		{
			logger.LogException(ex);

			return Result.InternalServerError<TokenDTO>(stringLocalizer["InternalServerError"], ex);
		}
	}

	public async Task<Result<TokenDTO>> RefreshTokensAsync(TokenDTO tokenDTO, CancellationToken cancellationToken = default)
	{
		try
		{
			IEnumerable<Claim>? oldClaims = await ValidateAccessTokenAsync(tokenDTO.AccessToken, secret, issuer, audience).ConfigureAwait(false);

			if (oldClaims is null)
			{
				logger.LogInformation("oldClaims is null.");

				return Result.Unauthorized<TokenDTO>(stringLocalizer["Unauthorized"]);
			}

			AuthState? refreshToken = await authStateRepository.GetByRefreshTokenAsync(tokenDTO.RefreshToken, cancellationToken).ConfigureAwait(false);

			if (refreshToken is null || refreshToken.LockoutEndUTC > DateTimeOffset.UtcNow || refreshToken.RefreshTokentExpiryUTC < DateTimeOffset.UtcNow)
			{
				logger.LogInformation("No existing refreshtoken or locked out or refresh token expired.");

				return Result.Unauthorized<TokenDTO>(stringLocalizer["Unauthorized"]);
			}

			refreshToken.RefreshToken = Guid.NewGuid().ToString();
			refreshToken.RefreshTokentExpiryUTC = DateTime.UtcNow.AddDays(refreshTokenExpiryInDays);

			await authStateRepository.UpsertAsync(refreshToken, cancellationToken).ConfigureAwait(false);

			List<Claim> newClaims = new(oldClaims.Where(x => x.Type is not JwtRegisteredClaimNames.Jti))
			{
				new(JwtRegisteredClaimNames.Jti, refreshToken.RefreshToken)
			};

			string newAccessToken = GenerateAccessToken(newClaims);
			
			logger.LogInformation("Tokens are refreshed | Old values: ({oldAccessToken}, {oldRefreshToken}) | New values: ({newAccessToken}, {newRefreshToken}).", tokenDTO.AccessToken, tokenDTO.RefreshToken, newAccessToken, refreshToken.RefreshToken);

			return Result.Ok(new TokenDTO(newAccessToken, refreshToken.RefreshToken));
		}
		catch (Exception ex)
		{
			logger.LogException(ex);

			return Result.InternalServerError<TokenDTO>(stringLocalizer["InternalServerError"], ex);
		}
	}

	private string GenerateAccessToken(IEnumerable<Claim> claims)
	{
		SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(secret));
		SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256Signature);
		JwtSecurityToken token = new(issuer: issuer, audience: !claims.Any(x => x.Type is JwtRegisteredClaimNames.Aud) ? audience : null, claims: claims, expires: DateTime.UtcNow.AddMinutes(accessTokenExpiryInMinutes), signingCredentials: credentials);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	private static async Task<IEnumerable<Claim>?> ValidateAccessTokenAsync(string accessToken, string secret, string issuer, string audience)
	{
		TokenValidationParameters tokenValidationParameters = new()
		{
			ValidateIssuerSigningKey = true,
			ValidateLifetime = false,
			ValidIssuer = issuer,
			ValidAudience = audience,
			ClockSkew = TimeSpan.Zero,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
		};

		TokenValidationResult validationResult = await new JwtSecurityTokenHandler().ValidateTokenAsync(accessToken, tokenValidationParameters);

		if (validationResult.SecurityToken is JwtSecurityToken jwtSecurityToken)
		{
			return jwtSecurityToken.Claims;
		}

		return null;
	}

	public async Task<Result> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
	{
		try
		{
			AuthState? refreshTokenEntity = await authStateRepository.GetByRefreshTokenAsync(refreshToken, cancellationToken).ConfigureAwait(false);

			if (refreshTokenEntity is null
				|| !BCryptNext.Verify(refreshToken, refreshTokenEntity.RefreshToken)
				|| refreshTokenEntity.RefreshTokentExpiryUTC < DateTimeOffset.UtcNow
				|| refreshTokenEntity.LockoutEndUTC < DateTimeOffset.UtcNow)
			{
				return Result.Unauthorized(stringLocalizer["Unauthorized"]);
			}

			return Result.Ok();
		}
		catch (Exception ex)
		{
			logger.LogException(ex);

			return Result.InternalServerError(stringLocalizer["InternalServerError"], ex);
		}
	}

	public async Task<Result> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
	{
		try
		{
			AuthState? refreshTokenEntity = await authStateRepository.GetByRefreshTokenAsync(refreshToken, cancellationToken).ConfigureAwait(false);

			if (refreshTokenEntity is null)
			{
				return Result.NotFound(stringLocalizer["NotFound", "RefreshToken"]);
			}

			refreshTokenEntity.LockoutEndUTC = DateTime.UtcNow;

			await authStateRepository.UpsertAsync(refreshTokenEntity, cancellationToken).ConfigureAwait(false);

			return Result.Ok();
		}
		catch (Exception ex)
		{
			logger.LogException(ex);

			return Result.InternalServerError(stringLocalizer["InternalServerError"], ex);
		}
	}

	public async Task<Result> RevokeUserAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		try
		{
			IEnumerable<AuthState> refreshTokens = await authStateRepository.GetRefreshTokensByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);

			foreach (AuthState refreshToken in refreshTokens)
			{
				refreshToken.LockoutEndUTC = DateTime.UtcNow;
			}

			await authStateRepository.UpdateRangeAsync(refreshTokens, cancellationToken).ConfigureAwait(false);

			return Result.Ok();
		}
		catch (Exception ex)
		{
			logger.LogException(ex);

			return Result.InternalServerError(stringLocalizer["InternalServerError"], ex);
		}
	}
}