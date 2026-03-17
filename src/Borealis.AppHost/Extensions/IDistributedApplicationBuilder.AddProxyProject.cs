using Projects;

namespace Borealis.AppHost.Extensions;

public static partial class IDistributedApplicationBuilderExtensions {
    public static IDistributedApplicationBuilder AddProxyProject(
            this IDistributedApplicationBuilder builder,
            IResourceBuilder<ProjectResource> web) {
        if(builder.ExecutionContext.IsRunMode) {
            var proxyProject = builder.AddAzureFunctionsProject<Borealis_WhiteoutSurvivalProxy>("borealis-wos-proxy");
            web.WithReference(proxyProject);
        } else {
            var proxyUrl = builder.AddParameter("borealis-wos-proxy-url");
            var proxyProject = builder.AddExternalService("borealis-wos-proxy", proxyUrl);
            web.WithReference(proxyProject);
        }

        return builder;
    }
}
