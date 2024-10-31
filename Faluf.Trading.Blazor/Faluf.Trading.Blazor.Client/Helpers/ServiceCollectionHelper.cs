using Faluf.Trading.Blazor.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Serilog;
using Serilog.Events;

namespace Faluf.Trading.Blazor.Client.Helpers;

internal static class ServiceCollectionHelper
{
	public static void AddTradingCore(this WebAssemblyHostBuilder builder)
	{
		builder.Logging.SetMinimumLevel(LogLevel.None);

		LoggerConfiguration loggerConfiguration = new();

		loggerConfiguration.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);
		loggerConfiguration.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning);

		loggerConfiguration.WriteTo.BrowserHttp($"{builder.HostEnvironment.BaseAddress}ingest");

		Log.Logger = loggerConfiguration.CreateLogger();

		builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
	}

	public static void AddTradingAuthentication(this IServiceCollection services)
	{
		services.AddAuthorizationCore();
		services.AddCascadingAuthenticationState();
		services.AddSingleton<AuthenticationStateProvider, ClientAuthenticationStateProvider>();
	}

	public static void AddTradingServices(this IServiceCollection services)
	{
		static void APIClient(HttpClient client) => client.BaseAddress = new(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ? "https://localhost:7235/" : "https://localhost:7235/"); // TODO

		services.AddHttpClient<IAuthService, ClientAuthService>(APIClient);
		services.AddHttpClient<IUserService, ClientUserService>(APIClient);
	}
}