using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BitzArt.Blazor.Cookies;
using FluentValidation;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCryptNext = BCrypt.Net.BCrypt;

namespace Faluf.Trading.Infrastructure.Services;

public sealed class AuthService(ICookieService cookieService, AuthenticationStateProvider authenticationStateProvider, IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, ILogger<AuthService> logger, IStringLocalizer<AuthService> stringLocalizer, IConfiguration configuration) : IAuthService
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
				return Result.Unauthorized<TokenDTO>(stringLocalizer["BadCredentials"]);
			}

			DateTimeOffset? dateTimeLockoutEnd = await userRepository.GetLockoutEndDateAsync(user, cancellationToken).ConfigureAwait(false);

			if (dateTimeLockoutEnd > DateTimeOffset.Now)
			{
				TimeSpan lockoutEnd = (dateTimeLockoutEnd - DateTimeOffset.Now).Value;
				double lockoutEndMinutes = Math.Ceiling(lockoutEnd.TotalMinutes);
				double lockoutEndSeconds = Math.Ceiling(lockoutEnd.TotalSeconds);

				return Result.Locked<TokenDTO>(stringLocalizer["LockoutMessage", lockoutEndMinutes, lockoutEndSeconds]);
			}

			bool isValidPassword = BCryptNext.Verify(loginInputModel.Password, user.HashedPassword);

			if (!isValidPassword)
			{
				user.AccessFailedCount++;

				if (user.AccessFailedCount >= 5)
				{
					user.LockoutEndUTC = DateTime.UtcNow.AddMinutes(15);
					user.AccessFailedCount = 0;
				}

				await userRepository.UpsertAsync(user, cancellationToken).ConfigureAwait(false);

				return Result.Unauthorized<TokenDTO>(stringLocalizer["BadCredentials", user.AccessFailedCount]);
			}

			string jti = Guid.NewGuid().ToString();

			List<Claim> claims =
			[
				new(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new(ClaimTypes.Name, user.FirstName),
				new(ClaimTypes.Surname, user.LastName),
				new(ClaimTypes.Email, user.Email),
				new(JwtRegisteredClaimNames.Jti, jti)
			];

			foreach (string role in user.Roles)
			{
				claims.Add(new(ClaimTypes.Role, role));
			}

			RefreshToken? currentRefreshToken = await refreshTokenRepository.GetByRefreshTokenAsync(jti, cancellationToken);

			currentRefreshToken ??= new();
			currentRefreshToken.UserId = user.Id;
			currentRefreshToken.Token = jti;
			currentRefreshToken.ExpiresAtUTC = DateTime.UtcNow.AddDays(refreshTokenExpiryInDays);
			currentRefreshToken.LoginFrom = loginInputModel.ClientType;

			await refreshTokenRepository.UpsertAsync(currentRefreshToken, cancellationToken).ConfigureAwait(false);

			user.AccessFailedCount = 0;

			await userRepository.UpsertAsync(user, cancellationToken).ConfigureAwait(false);

			string accessToken = GenerateAccessToken(claims);

			await cookieService.SetAsync("accessToken", accessToken, loginInputModel.IsRememberMeChecked ? DateTime.Now.AddYears(1) : null, cancellationToken);
			await cookieService.SetAsync("rememberMe", loginInputModel.IsRememberMeChecked.ToString(), DateTime.Now.AddYears(1), cancellationToken);

			((RevalidatingServerAuthenticationStateProvider)authenticationStateProvider).SetAuthenticationState(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")))));

			return Result.Ok(new TokenDTO(accessToken, jti));
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
				return Result.Unauthorized<TokenDTO>(stringLocalizer["Unauthorized"]);
			}

			RefreshToken? refreshToken = await refreshTokenRepository.GetByRefreshTokenAsync(tokenDTO.RefreshToken, cancellationToken).ConfigureAwait(false);

			if (refreshToken is null || refreshToken.RevokedAtUTC > DateTimeOffset.UtcNow)
			{
				return Result.Unauthorized<TokenDTO>(stringLocalizer["Unauthorized"]);
			}

			string newJti = Guid.NewGuid().ToString();

			refreshToken.Token = newJti;
			refreshToken.ExpiresAtUTC = DateTime.UtcNow.AddDays(refreshTokenExpiryInDays);

			await refreshTokenRepository.UpsertAsync(refreshToken, cancellationToken).ConfigureAwait(false);

			List<Claim> newClaims = new(oldClaims.Where(x => x.Type is not JwtRegisteredClaimNames.Jti))
			{
				new(JwtRegisteredClaimNames.Jti, newJti)
			};

			return Result.Ok(new TokenDTO(GenerateAccessToken(newClaims), newJti));
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
			RefreshToken? refreshTokenEntity = await refreshTokenRepository.GetByRefreshTokenAsync(refreshToken, cancellationToken).ConfigureAwait(false);

			if (refreshTokenEntity is null
				|| !BCryptNext.Verify(refreshToken, refreshTokenEntity.Token)
				|| refreshTokenEntity.ExpiresAtUTC < DateTimeOffset.UtcNow
				|| refreshTokenEntity.RevokedAtUTC < DateTimeOffset.UtcNow)
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
			RefreshToken? refreshTokenEntity = await refreshTokenRepository.GetByRefreshTokenAsync(refreshToken, cancellationToken).ConfigureAwait(false);

			if (refreshTokenEntity is null)
			{
				return Result.NotFound(stringLocalizer["NotFound", "RefreshToken"]);
			}

			refreshTokenEntity.RevokedAtUTC = DateTime.UtcNow;

			await refreshTokenRepository.UpsertAsync(refreshTokenEntity, cancellationToken).ConfigureAwait(false);

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
			IEnumerable<RefreshToken> refreshTokens = await refreshTokenRepository.GetRefreshTokensByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);

			foreach (RefreshToken refreshToken in refreshTokens)
			{
				refreshToken.RevokedAtUTC = DateTime.UtcNow;
			}

			await refreshTokenRepository.UpdateRangeAsync(refreshTokens, cancellationToken).ConfigureAwait(false);

			return Result.Ok();
		}
		catch (Exception ex)
		{
			logger.LogException(ex);

			return Result.InternalServerError(stringLocalizer["InternalServerError"], ex);
		}
	}
}