using System.Data;
using System.Reflection;
using System.Text;
using Faluf.Trading.Blazor.Services;
using Faluf.Trading.Core.Interfaces.Repositories;
using Faluf.Trading.Infrastructure.Repositories;
using Faluf.Trading.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace Faluf.Trading.Blazor.Helpers;

internal static class ServiceCollectionHelper
{
	public static void AddTradingCore(this WebApplicationBuilder builder)
	{
		// Logging
		builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
		{
			loggerConfiguration.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);
			loggerConfiguration.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning);

			loggerConfiguration.WriteTo.Console(LogEventLevel.Information);
			loggerConfiguration.WriteTo.MSSqlServer(
				connectionString: builder.Configuration.GetConnectionString("TradingConnection")!,
				sinkOptions: new MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true },
				restrictedToMinimumLevel: LogEventLevel.Warning,
				columnOptions: new ColumnOptions { PrimaryKey = new SqlColumn { ColumnName = "Id", DataType = SqlDbType.UniqueIdentifier, AllowNull = false, NonClusteredIndex = true } });
		});

		// Localization
		builder.Services.AddLocalization();

		// Validations
		builder.Services.AddValidatorsFromAssembly(Assembly.Load("Faluf.Trading.Core"));

		// HttpContextAccessor for authentication
		builder.Services.AddHttpContextAccessor();
	}

	public static void AddTradingDatabase(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContextFactory<TradingDbContext>(options =>
		{
			options.UseSqlServer(configuration.GetConnectionString("TradingConnection")!, sqlOptions => sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
			options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
		});
	}

	public static void AddTradingAuthentication(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddCascadingAuthenticationState();
		services.AddScoped<AuthenticationStateProvider, JWTAuthenticationStateProvider>();
		services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		}).AddJwtBearer(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateLifetime = false,
				ValidateIssuerSigningKey = true,
				ValidIssuer = configuration["JWT:Issuer"]!,
				ValidAudience = configuration["JWT:Audience"]!,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!)),
				ClockSkew = TimeSpan.Zero
			};

			options.Events = new JwtBearerEvents
			{
				OnMessageReceived = context =>
				{
					string? accessToken = context.Request.Cookies["accessToken"];

					if (!string.IsNullOrWhiteSpace(accessToken))
					{
						IDataProtector dataProtector = context.HttpContext.RequestServices.GetRequiredService<IDataProtectionProvider>().CreateProtector("AuthService");
						context.Token = dataProtector.Unprotect(accessToken);
					}

					return Task.CompletedTask;
				}
			};
		});
	}

	public static void AddTradingRepositories(this IServiceCollection services)
	{
		services.AddScoped<IUserRepository, UserRepository>();
		services.AddScoped<IAuthStateRepository, AuthStateRepository>();
	}

	public static void AddTradingServices(this IServiceCollection services)
	{
		services.AddScoped<IUserService, UserService>();
		services.AddScoped<IAuthService, AuthService>();
	}
}