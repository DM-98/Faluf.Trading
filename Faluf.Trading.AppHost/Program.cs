IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Faluf_Trading_Blazor>("blazor-rendermode-auto").WithExternalHttpEndpoints();

builder.Build().Run();