using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace Faluf.Trading.Blazor.Services;

// This is a server-side AuthenticationStateProvider that revalidates the security stamp for the connected user
// every 30 minutes an interactive circuit is connected.
public sealed class RevalidatingAuthenticationStateProvider(ILoggerFactory loggerFactory, IServiceScopeFactory serviceScopeFactory) : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
	protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

	protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
	{
		await using AsyncServiceScope scope = serviceScopeFactory.CreateAsyncScope();
		IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

		return await ValidateRefreshTokenAsync(authService, authenticationState.User);
	}

	private static async Task<bool> ValidateRefreshTokenAsync(IAuthService authService, ClaimsPrincipal principal)
	{
        string? jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);

        if (string.IsNullOrWhiteSpace(jti))
		{
			return false;
		}

		Result refreshTokenResult = await authService.ValidateRefreshTokenAsync(jti, cancellationToken: CancellationToken.None);

        return refreshTokenResult.IsSuccess;
	}
}