var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.AspireSample_ApiService>("apiservice")
    .WithEndpoint(name: "dashboard", env: "APIDASHBOARDPORT", isProxied: true, isExternal: true, scheme: "http")
    .WithReplicas(2);

builder.AddProject<Projects.AspireSample_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
