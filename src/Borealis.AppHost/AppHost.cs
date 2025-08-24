var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("project-borealis")
    .WithDashboard(options => {
        options.WithExternalHttpEndpoints();
    });

var discordBotToken = builder.AddParameter("discord-bot-token");
var discordClientSecret = builder.AddParameter("discord-client-secret");
var discordClientId = builder.AddParameter("discord-client-id");

var borealisApplicationUrl = builder.AddParameter("borealis-application-url");

var postgresUsername = builder.AddParameter("postgres-user", "postgres");
var postgresPassword = builder.AddParameter("postgres-password", "postgres");

var postgres = builder
    .AddPostgres("borealis-postgres", postgresUsername, postgresPassword)
    .WithPassword(postgresPassword)
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
        service.Restart = "no";
    });

var pgLoader = builder.AddDockerfile("borealis-pgloader", "docker/pgloader")
    .WithReference(postgresdb, "PostgresDb")
    .WithBindMount(source: "./docker/pgloader/db", "/sqlite/db")
    .WaitFor(postgresdb)
    .WaitForCompletion(migrations)
    .PublishAsDockerComposeService((_, service) => {
        service.Restart = "no";
    });

var web = builder
    .AddProject<Projects.Borealis_Web>("borealis-web")
    .WithExternalHttpEndpoints()
    .WithEnvironment("Discord__BotToken", discordBotToken)
    .WithEnvironment("Discord__ClientSecret", discordClientSecret)
    .WithEnvironment("Discord__ClientId", discordClientId)
    .WithEnvironment("Borealis__ApplicationUrl", borealisApplicationUrl)
    .WithReference(postgresdb, "PostgresDb")
    .WaitForCompletion(migrations)
    .WaitFor(postgresdb)
    .PublishAsDockerFile(options => {
        options.WithDockerfile("../.."); // This can't have an ending slash
    })
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
