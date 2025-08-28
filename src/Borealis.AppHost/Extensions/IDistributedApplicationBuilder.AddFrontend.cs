namespace Borealis.AppHost.Extensions;

public static partial class IDistributedApplicationBuilderExtensions {
    public static IDistributedApplicationBuilder AddFrontend(
            this IDistributedApplicationBuilder builder,
            IResourceBuilder<ProjectResource> web) {
        if(builder.ExecutionContext.IsRunMode) {
            var frontend = builder.AddNpmApp("frontend", "../Borealis.Frontend", "dev");

            web.WaitFor(frontend);
        }

        return builder;
    }
}
