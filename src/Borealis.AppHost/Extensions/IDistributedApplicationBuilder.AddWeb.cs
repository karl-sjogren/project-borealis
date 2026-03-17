using Projects;

namespace Borealis.AppHost.Extensions;

public static partial class IDistributedApplicationBuilderExtensions {
    public static IResourceBuilder<ProjectResource> AddWeb(
            this IDistributedApplicationBuilder builder,
            IResourceBuilder<PostgresDatabaseResource> postgresdb,
            IResourceBuilder<ProjectResource> migrations) {
        var web = builder
            .AddProject<Borealis_Web>("borealis-web")
            .WithExternalHttpEndpoints()
            .WithReference(postgresdb, "PostgresDb")
            .WaitForCompletion(migrations)
            .WaitFor(postgresdb)
            .WithUrlForEndpoint("http", u => u.DisplayLocation = UrlDisplayLocation.DetailsOnly)
            .PublishAsDockerFile(options => {
                options.WithDockerfile("../.."); // This can't have an ending slash
            })
            .PublishAsDockerComposeService((_, service) => {
                service.Restart = "unless-stopped";
            });

        web
            .AddEnvironmentVariables(builder)
            .AddExternalResources(builder);

        return web;
    }

    private static IResourceBuilder<ProjectResource> AddEnvironmentVariables(
            this IResourceBuilder<ProjectResource> web,
            IDistributedApplicationBuilder builder) {
        var discordBotToken = builder.AddParameter("discord-bot-token", secret: true);
        var discordClientSecret = builder.AddParameter("discord-client-secret", secret: true);
        var discordClientId = builder.AddParameter("discord-client-id");

        var borealisApplicationUrl = builder.AddParameter("borealis-application-url", "https://borealis.karl-sjogren.com/");
        var whiteoutSurvivalProxyFunctionKey = builder.AddParameter("whiteout-survival-proxy-function-key", secret: true);

        web
            .WithEnvironment("Discord__BotToken", discordBotToken)
            .WithEnvironment("Discord__ClientSecret", discordClientSecret)
            .WithEnvironment("Discord__ClientId", discordClientId)
            .WithEnvironment("Borealis__ApplicationUrl", borealisApplicationUrl)
            .WithEnvironment("WhiteoutSurvivalProxy__FunctionKey", whiteoutSurvivalProxyFunctionKey);

        return web;
    }

    private static IResourceBuilder<ProjectResource> AddExternalResources(
            this IResourceBuilder<ProjectResource> web,
            IDistributedApplicationBuilder builder) {
        var whiteoutBotService = builder.AddExternalService("whiteout-bot", new Uri("http://gift-code-api.whiteout-bot.com/"));
        var destructoidService = builder.AddExternalService("destructoid", new Uri("https://www.destructoid.com/"));
        var wosgiftcodesService = builder.AddExternalService("wosgiftcodes", new Uri("https://wosgiftcodes.com/"));
        var wosrewardsService = builder.AddExternalService("wosrewards", new Uri("https://wosrewards.com/"));
        var wosGiftcodeApiService = builder.AddExternalService("wos-giftcode-api", new Uri("https://wos-giftcode-api.centurygame.com/"));

        web
            .WithReference(whiteoutBotService)
            .WithReference(destructoidService)
            .WithReference(wosgiftcodesService)
            .WithReference(wosrewardsService)
            .WithReference(wosGiftcodeApiService);

        return web;
    }
}
