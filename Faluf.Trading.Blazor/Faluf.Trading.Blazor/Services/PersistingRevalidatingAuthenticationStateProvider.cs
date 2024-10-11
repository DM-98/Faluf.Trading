using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Faluf.Trading.Blazor.Services;

// This is a server-side AuthenticationStateProvider that revalidates the security stamp for the connected user
// every 30 minutes an interactive circuit is connected. It also uses PersistentComponentState to flow the
// authentication state to the client which is then fixed for the lifetime of the WebAssembly application.
internal sealed class PersistingRevalidatingAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
	private readonly IServiceScopeFactory scopeFactory;
	private readonly PersistentComponentState state;
	private readonly PersistingComponentStateSubscription subscription;

	private Task<AuthenticationState>? authenticationStateTask;

	public PersistingRevalidatingAuthenticationStateProvider(ILoggerFactory loggerFactory, IServiceScopeFactory serviceScopeFactory, PersistentComponentState persistentComponentState) : base(loggerFactory)
	{
		scopeFactory = serviceScopeFactory;
		state = persistentComponentState;
		subscription = state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveWebAssembly);

		AuthenticationStateChanged += OnAuthenticationStateChanged;
	}

	protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

	protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
	{
		await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
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

	private void OnAuthenticationStateChanged(Task<AuthenticationState> task) => authenticationStateTask = task;

	private async Task OnPersistingAsync()
	{
		if (authenticationStateTask is null)
		{
			throw new UnreachableException($"Authentication state not set in {nameof(OnPersistingAsync)}().");
		}

		AuthenticationState authenticationState = await authenticationStateTask;
		ClaimsPrincipal principal = authenticationState.User;

		if (principal.Identity?.IsAuthenticated == true)
		{
			string? userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            string? firstName = principal.FindFirstValue(ClaimTypes.Name);
            string? lastName = principal.FindFirstValue(ClaimTypes.Surname);
            string? email = principal.FindFirstValue(ClaimTypes.Email);

			if (userId is not null && email is not null && firstName is not null && lastName is not null)
			{
                state.PersistAsJson("userInfo", new Core.DTOs.Outputs.UserInfo
                {
                    UserId = userId,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName
                });
            }
		}
	}

	protected override void Dispose(bool disposing)
	{
		subscription.Dispose();
		AuthenticationStateChanged -= OnAuthenticationStateChanged;

		base.Dispose(disposing);
	}
}