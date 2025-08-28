using Borealis.AppHost.Extensions;

namespace Borealis.AppHost.Extensions;

public static partial class IDistributedApplicationBuilderExtensions {
    public static IResourceBuilder<PostgresDatabaseResource> AddDatabase(
            this IDistributedApplicationBuilder builder) {
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

        return postgresdb;
    }
}
