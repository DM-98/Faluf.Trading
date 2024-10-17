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
		static void APIClient(HttpClient client) => client.BaseAddress = new(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ? "https://localhost:7235/" : "https://localhost:7235/"); // TODO

		services.AddHttpClient<IAuthService, ClientAuthService>(APIClient);
		services.AddHttpClient<IUserService, ClientUserService>(APIClient);

		return services;
	}
}