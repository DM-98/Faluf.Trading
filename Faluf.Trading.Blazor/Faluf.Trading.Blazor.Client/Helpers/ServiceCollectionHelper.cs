using Faluf.Trading.Blazor.Client.Services;

namespace Faluf.Trading.Blazor.Client.Helpers;

public static class ServiceCollectionHelper
{
	public static IServiceCollection AddTradingAuthentication(this IServiceCollection services)
	{
		services.AddAuthorizationCore();
		services.AddCascadingAuthenticationState();
		services.AddAuthenticationStateDeserialization();

		return services;
	}

	public static IServiceCollection AddTradingServices(this IServiceCollection services)
	{
		services.AddHttpClient<IAuthService, HttpClientAuthService>(c => c.BaseAddress = new Uri("https://localhost:7235/"));

		return services;
	}
}