using Projects;

namespace Borealis.AppHost.Extensions;

public static partial class IDistributedApplicationBuilderExtensions {
    public static IResourceBuilder<ProjectResource> AddMigrationProject(
            this IDistributedApplicationBuilder builder,
            IResourceBuilder<PostgresDatabaseResource> postgresdb) {
        var migrations = builder.AddProject<Borealis_MigrationService>("borealis-migrations")
            .WithReference(postgresdb, "PostgresDb")
            .WaitFor(postgresdb)
            .PublishAsDockerComposeService((_, service) => service.Restart = "no");

        return migrations;
    }
}
