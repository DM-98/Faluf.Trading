using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BitzArt.Blazor.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace Faluf.Trading.Blazor.Services;

// This is a server-side AuthenticationStateProvider that revalidates the security stamp for the connected user
// every 5 minutes an interactive circuit is connected.
public sealed class RevalidatingAuthenticationStateProvider(ICookieService cookieService, ILoggerFactory loggerFactory, IServiceScopeFactory serviceScopeFactory) : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
	protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(5);

	private static readonly AuthenticationState anonAuthState = new(new(new ClaimsIdentity()));

	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		Cookie? accessTokenCookie = await cookieService.GetAsync("accessToken");

		if (accessTokenCookie is null)
		{
			return anonAuthState;
		}

		IEnumerable<Claim> claims = new JwtSecurityTokenHandler().ReadJwtToken(accessTokenCookie.Value).Claims;

		await using AsyncServiceScope scope = serviceScopeFactory.CreateAsyncScope();
		IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

		DateTimeOffset accessTokenExpiry = DateTimeOffset.FromUnixTimeSeconds(long.Parse(claims.First(x => x.Type == "exp").Value));

		if (accessTokenExpiry < DateTimeOffset.UtcNow)
		{
			string refreshToken = claims.FirstOrDefault(x => x.Type is JwtRegisteredClaimNames.Jti)?.Value!;

			Result<TokenDTO> refreshTokensResult = await authService.RefreshTokensAsync(new TokenDTO(accessTokenCookie.Value, refreshToken));

			if (!refreshTokensResult.IsSuccess)
			{
				return anonAuthState;
			}

			IEnumerable<Claim> newClaims = new JwtSecurityTokenHandler().ReadJwtToken(refreshTokensResult.Content.AccessToken).Claims;
			ClaimsPrincipal claimsPrincipal = new(new ClaimsIdentity(newClaims, "jwt"));
			SetAuthenticationState(Task.FromResult(new AuthenticationState(claimsPrincipal)));
			
			bool isRememberMe = await cookieService.GetAsync("rememberMe") is { Value: "True" };

			await cookieService.SetAsync("accessToken", refreshTokensResult.Content.AccessToken, isRememberMe ? DateTime.Now.AddYears(1) : null);

			return new AuthenticationState(claimsPrincipal);
		}

		return new(new(new ClaimsIdentity(claims, "jwt")));
	}

	protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken) => (await GetAuthenticationStateAsync()).User.Identity?.IsAuthenticated == true;
}