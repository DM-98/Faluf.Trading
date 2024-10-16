using BitzArt.Blazor.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents().AddInteractiveWebAssemblyComponents().AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);
builder.Services.AddControllers();
builder.Services.AddDataProtection().PersistKeysToDbContext<TradingDbContext>();

builder.AddBlazorCookies();
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
app.UseAntiforgery();

string[] supportedCultures = ["en-US", "da-DK"];
app.UseRequestLocalization(new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[0]).AddSupportedCultures(supportedCultures).AddSupportedUICultures(supportedCultures));

app.UseJWTAuthorizationMiddleware();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode().AddInteractiveWebAssemblyRenderMode().AddAdditionalAssemblies(typeof(Faluf.Trading.Blazor.Client._Imports).Assembly);
app.MapControllers();

app.Run();