using Faluf.Trading.Blazor.Helpers;
using Microsoft.AspNetCore.DataProtection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents().AddInteractiveServerComponents().AddInteractiveWebAssemblyComponents();
builder.Services.AddControllers();
builder.Services.AddCors(options => options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddDataProtection().PersistKeysToDbContext<TradingDbContext>();

// Trading DI
builder.AddTradingCore();
builder.Services.AddTradingAuthentication(builder.Configuration);
builder.Services.AddTradingDatabase(builder.Configuration);
builder.Services.AddTradingRepositories();
builder.Services.AddTradingServices();

builder.Services.AddOpenApi();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

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

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.MapStaticAssets();
app.UseStaticFiles();
app.UseAntiforgery();

string[] supportedCultures = ["en-US", "da-DK"];
app.UseRequestLocalization(new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[0]).AddSupportedCultures(supportedCultures).AddSupportedUICultures(supportedCultures));

app.MapRazorComponents<App>().AddInteractiveServerRenderMode().AddInteractiveWebAssemblyRenderMode().AddAdditionalAssemblies(typeof(Faluf.Trading.Blazor.Client._Imports).Assembly);
app.MapControllers();

app.Run();