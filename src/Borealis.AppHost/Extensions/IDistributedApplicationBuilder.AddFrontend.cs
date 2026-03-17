namespace Borealis.AppHost.Extensions;

public static partial class IDistributedApplicationBuilderExtensions {
    public static IDistributedApplicationBuilder AddFrontend(
            this IDistributedApplicationBuilder builder,
            IResourceBuilder<ProjectResource> web) {
        if(builder.ExecutionContext.IsRunMode) {
            var frontend = builder.AddViteApp("frontend", "../Borealis.Frontend", "dev")
                .WithYarn();

            web.WaitFor(frontend);
        }

        return builder;
    }
}
