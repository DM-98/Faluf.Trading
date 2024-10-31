using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;

namespace Faluf.Trading.Blazor.Services;

public sealed class JWTAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
	private AuthenticationState authenticationState = new(new ClaimsPrincipal(new ClaimsIdentity()));
	private readonly PersistentComponentState persistentComponentState;
	private readonly IAuthService authService;
	private readonly PersistingComponentStateSubscription subscription;

	public JWTAuthenticationStateProvider(PersistentComponentState persistentComponentState, IAuthService authService)
	{
		this.persistentComponentState = persistentComponentState;
		this.authService = authService;

		AuthenticationStateChanged += OnAuthenticationStateChanged;
		subscription = this.persistentComponentState.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveWebAssembly);
	}

	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		Result<IEnumerable<Claim>> response = await authService.GetCurrentClaimsAsync();

		if (!response.IsSuccess)
		{
			return authenticationState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
		}

		DateTimeOffset expiration = DateTimeOffset.FromUnixTimeSeconds(long.Parse(response.Content.First(x => x.Type is JwtRegisteredClaimNames.Exp).Value));

		if (expiration > DateTimeOffset.UtcNow)
		{
			return authenticationState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(response.Content, "jwt")));
		}

		response = await authService.RefreshTokensAsync();

		authenticationState = response.IsSuccess ? new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(response.Content, "jwt"))) : new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
		
		NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));

		return authenticationState;
	}

	private Task OnPersistingAsync()
	{
		ClaimsPrincipal claimsPrincipal = authenticationState.User;

		if (claimsPrincipal.Identity is { IsAuthenticated: true })
		{
			persistentComponentState.PersistAsJson(nameof(AuthenticationStateData), new AuthenticationStateData
			{
				Claims = claimsPrincipal.Claims.Select(x => new ClaimData(x.Type, x.Value)).ToList(),
				NameClaimType = ClaimTypes.Name,
				RoleClaimType = ClaimTypes.Role
			});
		}

		return Task.CompletedTask;
	}

	private async void OnAuthenticationStateChanged(Task<AuthenticationState> authenticationStateTask) => authenticationState = await authenticationStateTask;

	public void Dispose()
	{
		subscription.Dispose();
		AuthenticationStateChanged -= OnAuthenticationStateChanged;
	}
}