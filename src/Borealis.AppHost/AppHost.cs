using Borealis.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

builder.SetupDockerCompose();

var postgresdb = builder.AddDatabase();

var migrations = builder.AddMigrationProject(postgresdb);

var web = builder.AddWeb(postgresdb, migrations);

builder
    .AddProxyProject(web)
    .AddFrontend(web);

builder.Build().Run();
