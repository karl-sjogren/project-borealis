namespace Borealis.AppHost.Extensions;

public static partial class IDistributedApplicationBuilderExtensions {
    public static IDistributedApplicationBuilder SetupDockerCompose(
            this IDistributedApplicationBuilder builder) {
        var migrations = builder.AddDockerComposeEnvironment("project-borealis")
            .WithDashboard(options => options.WithExternalHttpEndpoints());

        return builder;
    }
}
