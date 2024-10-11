using Faluf.Trading.Blazor.Helpers;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents().AddInteractiveServerComponents().AddInteractiveWebAssemblyComponents();

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
	app.UseMigrationsEndPoint();
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("openapi/v1.json", "API v1"));
}
else
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}

app.UseHttpsRedirection();

app.MapStaticAssets();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode().AddInteractiveWebAssemblyRenderMode().AddAdditionalAssemblies(typeof(Faluf.Trading.Blazor.Client._Imports).Assembly);

app.Run();