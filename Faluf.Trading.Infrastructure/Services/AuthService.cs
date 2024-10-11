using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCryptNext = BCrypt.Net.BCrypt;

namespace Faluf.Trading.Infrastructure.Services;

public sealed class AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, ILogger<AuthService> logger, IStringLocalizer<AuthService> stringLocalizer, IConfiguration configuration) : IAuthService
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

            bool isValidPassword = BCryptNext.Verify(user.HashedPassword, loginInputModel.Password);

            if (!isValidPassword)
            {
                int newAccessFailedCount = await userRepository.IncrementAccessFailedCountAsync(user, cancellationToken).ConfigureAwait(false);

                return Result.Unauthorized<TokenDTO>(stringLocalizer["BadCredentials", newAccessFailedCount]);
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

            RefreshToken refreshToken = new()
            {
                UserId = user.Id,
                HashedToken = BCryptNext.HashPassword(jti),
                ExpiresAtUTC = DateTime.UtcNow.AddDays(refreshTokenExpiryInDays)
            };

            refreshToken = await refreshTokenRepository.CreateAsync(refreshToken, cancellationToken).ConfigureAwait(false);

            user.AccessFailedCount = 0;

            await userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);

            return Result.Ok(new TokenDTO(GenerateAccessToken(claims), jti));
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
            
            string jti = oldClaims.First(x => x.Type is JwtRegisteredClaimNames.Jti).Value;
            RefreshToken? refreshToken = await refreshTokenRepository.GetByRefreshTokenAsync(jti, cancellationToken).ConfigureAwait(false);

            if (refreshToken is null || DateTimeOffset.UtcNow >= refreshToken.ExpiresAtUTC || DateTimeOffset.UtcNow >= refreshToken.RevokedAtUTC)
            {
                return Result.Unauthorized<TokenDTO>(stringLocalizer["Unauthorized"]);
            }

            string newJti = Guid.NewGuid().ToString();

            refreshToken.HashedToken = BCryptNext.HashPassword(newJti);
            refreshToken.ExpiresAtUTC = DateTime.UtcNow.AddDays(refreshTokenExpiryInDays);

            await refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken).ConfigureAwait(false);
           
            List<Claim> newClaims = new(oldClaims.Where(x => x.Type is not JwtRegisteredClaimNames.Jti))
            {
                new(JwtRegisteredClaimNames.Jti, newJti)
            };

            return Result.Ok(new TokenDTO(GenerateAccessToken(newClaims), refreshToken.HashedToken));
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
                || !BCryptNext.Verify(refreshToken, refreshTokenEntity.HashedToken)
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

            await refreshTokenRepository.UpdateAsync(refreshTokenEntity, cancellationToken).ConfigureAwait(false);

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