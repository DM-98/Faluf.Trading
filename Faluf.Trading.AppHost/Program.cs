IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Faluf_Trading_Blazor>("faluf-trading-blazor");

builder.Build().Run();