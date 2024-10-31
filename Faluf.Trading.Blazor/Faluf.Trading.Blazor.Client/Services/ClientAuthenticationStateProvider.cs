using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Faluf.Trading.Blazor.Client.Services;

public sealed class ClientAuthenticationStateProvider : AuthenticationStateProvider
{
	private readonly Task<AuthenticationState> authenticationStateTask = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

	public ClientAuthenticationStateProvider(PersistentComponentState persistentComponentState)
	{
		if (!persistentComponentState.TryTakeFromJson(nameof(AuthenticationStateData), out AuthenticationStateData? authenticationStateData))
		{
			return;
		}

		authenticationStateTask = Task.FromResult(new AuthenticationState(new(new ClaimsIdentity(authenticationStateData!.Claims.Select(x => new Claim(x.Type, x.Value)), "jwt"))));
	}

	public override Task<AuthenticationState> GetAuthenticationStateAsync() => authenticationStateTask;
}