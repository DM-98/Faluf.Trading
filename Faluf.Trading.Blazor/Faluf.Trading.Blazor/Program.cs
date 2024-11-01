using System.Reflection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents().AddInteractiveWebAssemblyComponents().AddAuthenticationStateSerialization(x => x.SerializeAllClaims = true);
builder.Services.AddControllers();
builder.Services.AddDataProtection().PersistKeysToDbContext<TradingDbContext>();

builder.AddTradingCore();

builder.Services.AddTradingAuthentication(builder.Configuration);
builder.Services.AddTradingDatabase(builder.Configuration);

builder.Services.AddTradingRepositories();
builder.Services.AddTradingServices();

builder.Services.AddOpenApi();
builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseWebAssemblyDebugging();
	app.UseDeveloperExceptionPage();
	app.UseMigrationsEndPoint();
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
	{
		options.DefaultModelsExpandDepth(-1);
		options.SwaggerEndpoint("/openapi/v1.json", "API v1");
	});
}
else
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStatusCodePagesWithReExecute("/StatusCode/{0}");

app.UseAntiforgery();
app.UseSerilogIngestion();

string[] supportedCultures = ["en-US", "da-DK"];
app.UseRequestLocalization(new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[0]).AddSupportedCultures(supportedCultures).AddSupportedUICultures(supportedCultures));

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode().AddInteractiveWebAssemblyRenderMode().AddAdditionalAssemblies(Assembly.Load("Faluf.Trading.Blazor.Client")).AllowAnonymous();
app.MapControllers();

app.Run();