using Borealis.AppHost.Extensions;

#pragma warning disable ASPIRECOMPUTE003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var builder = DistributedApplication.CreateBuilder(args);

var registry = builder.AddContainerRegistry("ghcr", "ghcr.io", "karl-sjogren/project-borealis");

builder.SetupDockerCompose();

var postgresdb = builder.AddDatabase();

var migrations = builder.AddMigrationProject(postgresdb)
    .WithContainerRegistry(registry);

var web = builder.AddWeb(postgresdb, migrations)
    .WithContainerRegistry(registry);

builder
    .AddProxyProject(web)
    .AddFrontend(web);

builder.Build().Run();

#pragma warning restore ASPIRECOMPUTE003 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
