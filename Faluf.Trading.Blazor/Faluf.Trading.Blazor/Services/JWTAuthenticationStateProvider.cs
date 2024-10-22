using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BitzArt.Blazor.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

namespace Faluf.Trading.Blazor.Services;

public sealed class JWTAuthenticationStateProvider(ICookieService cookieService, IAuthService authService) : AuthenticationStateProvider
{
	private static readonly AuthenticationState anonAuthState = new(new(new ClaimsIdentity()));

	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		Cookie? accessTokenCookie = await cookieService.GetAsync("accessToken");

		if (accessTokenCookie is null)
		{
			return anonAuthState;
		}

		IEnumerable<Claim> claims = new JwtSecurityTokenHandler().ReadJwtToken(accessTokenCookie.Value).Claims;

		DateTimeOffset accessTokenExpiry = DateTimeOffset.FromUnixTimeSeconds(long.Parse(claims.First(x => x.Type == "exp").Value));

		if (accessTokenExpiry < DateTimeOffset.UtcNow)
		{
			string refreshToken = claims.First(x => x.Type is JwtRegisteredClaimNames.Jti).Value;

			Result<TokenDTO> refreshTokensResult = await authService.RefreshTokensAsync(new TokenDTO(accessTokenCookie.Value, refreshToken));

			if (!refreshTokensResult.IsSuccess)
			{
				await cookieService.RemoveAsync("accessToken");
				await cookieService.RemoveAsync("rememberMe");

				return anonAuthState;
			}

			bool isRememberMe = await cookieService.GetAsync("rememberMe") is { Value: "True" };
			await cookieService.SetAsync("accessToken", refreshTokensResult.Content.AccessToken, isRememberMe ? DateTime.Now.AddYears(1) : null);

			IEnumerable<Claim> newClaims = new JwtSecurityTokenHandler().ReadJwtToken(refreshTokensResult.Content.AccessToken).Claims;
			ClaimsPrincipal claimsPrincipal = new(new ClaimsIdentity(newClaims, "jwt"));
			NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));

			return new AuthenticationState(claimsPrincipal);
		}

		return new(new(new ClaimsIdentity(claims, "jwt")));
	}
}