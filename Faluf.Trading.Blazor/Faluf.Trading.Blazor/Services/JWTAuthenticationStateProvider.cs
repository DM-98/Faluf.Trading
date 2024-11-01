using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace Faluf.Trading.Blazor.Services;

public sealed class JWTAuthenticationStateProvider(IAuthService authService) : ServerAuthenticationStateProvider
{
	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		Result<IEnumerable<Claim>> response = await authService.GetCurrentClaimsAsync();

		if (!response.IsSuccess)
		{
			return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
		}

		if (long.Parse(response.Content.First(x => x.Type is JwtRegisteredClaimNames.Exp).Value) > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
		{
			return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(response.Content, "jwt")));
		}

		response = await authService.RefreshTokensAsync();

		return new AuthenticationState(new ClaimsPrincipal(response.IsSuccess ? new ClaimsIdentity(response.Content, "jwt") : new ClaimsIdentity()));
	}
}