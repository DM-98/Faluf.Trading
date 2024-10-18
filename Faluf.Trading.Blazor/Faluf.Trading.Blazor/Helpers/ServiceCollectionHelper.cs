using System.Data;
using System.Reflection;
using System.Text;
using Faluf.Trading.Blazor.Services;
using Faluf.Trading.Core.Interfaces.Repositories;
using Faluf.Trading.Infrastructure.Repositories;
using Faluf.Trading.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace Faluf.Trading.Blazor.Helpers;

public static class ServiceCollectionHelper
{
    public static WebApplicationBuilder AddTradingCore(this WebApplicationBuilder builder)
    {
        // Logging
        builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
        {
            loggerConfiguration.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);

			loggerConfiguration.WriteTo.Console(LogEventLevel.Information);

            loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: builder.Configuration.GetConnectionString("TradingConnection")!,
                sinkOptions: new() { TableName = "Logs", AutoCreateSqlTable = true },
                restrictedToMinimumLevel: LogEventLevel.Warning,
                columnOptions: new() { AdditionalColumns = [new SqlColumn("LogEvent", SqlDbType.NVarChar)] });

            loggerConfiguration.WriteTo.OpenTelemetry(options =>
            {
                string[] headers = builder.Configuration["OTEL_EXPORTER_OTLP_HEADERS"]?.Split(',') ?? [];

                foreach (string header in headers)
                {
                    string[] parts = header.Split('=');

                    (string key, string value) = parts.Length switch
                    {
                        2 => (parts[0], parts[1]),
                        _ => throw new Exception($"Invalid header format: {header}")
                    };

                    options.Headers.Add(key, value);
                }

                options.Endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
                options.ResourceAttributes.Add("service.name", "apiservice");
                options.RestrictedToMinimumLevel = LogEventLevel.Information;
            });
        });

		// Localization
		builder.Services.AddLocalization();

        // Validations
        builder.Services.AddValidatorsFromAssembly(Assembly.Load("Faluf.Trading.Core"));

        return builder;
    }

    public static IServiceCollection AddTradingDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextFactory<TradingDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("TradingConnection")!, sqlOptions => sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        return services;
    }

    public static IServiceCollection AddTradingAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCascadingAuthenticationState();
        services.AddScoped<AuthenticationStateProvider, RevalidatingAuthenticationStateProvider>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, BlazorAuthorizationMiddlewareResultHandler>();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters.ValidateIssuerSigningKey = true;
            options.TokenValidationParameters.ValidateLifetime = false;
            options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!));
            options.TokenValidationParameters.ValidAudience = configuration["JWT:Audience"]!;
            options.TokenValidationParameters.ValidIssuer = configuration["JWT:Issuer"]!;
            options.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
        });

        return services;
    }

    public static IServiceCollection AddTradingRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthStateRepository, AuthStateRepository>();

        return services;
    }

    public static IServiceCollection AddTradingServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}