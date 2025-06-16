var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050).WithImageTag("latest"));

var postgresdb = postgres.AddDatabase("borealis");

var frontend = builder.AddNpmApp("frontend", "../Borealis.Frontend", "dev");

var migrations = builder.AddProject<Projects.Borealis_MigrationService>("migrations")
    .WithReference(postgresdb, "PostgresDb")
    .WaitFor(postgresdb);

builder
    .AddProject<Projects.Borealis_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(postgresdb, "PostgresDb")
    .WaitFor(migrations)
    .WaitFor(frontend);

builder.Build().Run();
