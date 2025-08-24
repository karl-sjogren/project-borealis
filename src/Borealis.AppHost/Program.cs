var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("project-borealis")
    .WithDashboard();

var username = builder.AddParameter("postgres-user", "postgres");
var password = builder.AddParameter("postgres-password", "postgres");

var postgres = builder
    .AddPostgres("borealis-postgres", username, password)
    .WithPassword(password)
    .WithDataVolume(isReadOnly: false)
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050).WithImageTag("latest"))
    .PublishAsDockerComposeService((_, service) => {
        service.Restart = "unless-stopped";
        service.Healthcheck = new() {
            Interval = "10s",
            Timeout = "5s",
            Retries = 6,
            StartPeriod = "0s",
            Test = ["CMD-SHELL", "pg_isready -U postgres"]
        };
    });

var postgresdb = postgres.AddDatabase("borealis");

var migrations = builder.AddProject<Projects.Borealis_MigrationService>("borealis-migrations")
    .WithReference(postgresdb, "PostgresDb")
    .WaitFor(postgresdb)
    .PublishAsDockerComposeService((_, service) => {
        service.Restart = "unless-stopped";
    });

var pgLoader = builder.AddDockerfile("borealis-pgloader", "docker/pgloader")
    .WithReference(postgresdb, "PostgresDb")
    .WithBindMount(source: "./docker/pgloader/db", "/sqlite/db")
    .WaitFor(postgresdb)
    .WaitFor(migrations)
    .PublishAsDockerComposeService((_, service) => {
        service.Restart = "no";
    });

var web = builder
    .AddProject<Projects.Borealis_Web>("borealis-web")
    .WithExternalHttpEndpoints()
    .WithReference(postgresdb, "PostgresDb")
    .WaitFor(migrations)
    .WaitFor(postgresdb)
    .PublishAsDockerComposeService((_, service) => {
        service.Restart = "unless-stopped";
    });

if(builder.ExecutionContext.IsRunMode) {
    var frontend = builder.AddNpmApp("frontend", "../Borealis.Frontend", "dev");

    web.WaitFor(frontend);
} else if(builder.ExecutionContext.IsPublishMode) {
    // Build the frontend in publish mode?
}

builder.Build().Run();
