using Faluf.Trading.Blazor.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Refit;

namespace Faluf.Trading.Blazor.Client.Helpers;

public static class ServiceCollectionHelper
{
	public static IServiceCollection AddTradingAuthentication(this IServiceCollection services)
	{
		services.AddAuthorizationCore();
		services.AddCascadingAuthenticationState();
		services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

		return services;
	}

	public static IServiceCollection AddTradingServices(this IServiceCollection services)
	{
		services.AddRefitClient<IAuthService>().ConfigureHttpClient(c => c.BaseAddress = new Uri("https+http://blazor-rendermode-auto"));

		return services;
	}
}