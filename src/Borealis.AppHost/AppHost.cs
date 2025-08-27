using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("project-borealis")
    .WithDashboard(options => {
        options.WithExternalHttpEndpoints();
    });

var discordBotToken = builder.AddParameter("discord-bot-token", secret: true);
var discordClientSecret = builder.AddParameter("discord-client-secret", secret: true);
var discordClientId = builder.AddParameter("discord-client-id");

var borealisApplicationUrl = builder.AddParameter("borealis-application-url", "https://borealis.karl-sjogren.com/");
var whiteoutSurvivalProxyFunctionKey = builder.AddParameter("whiteout-survival-proxy-function-key", secret: true);

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

var migrations = builder.AddProject<Borealis_MigrationService>("borealis-migrations")
    .WithReference(postgresdb, "PostgresDb")
    .WaitFor(postgresdb)
    .PublishAsDockerComposeService((_, service) => service.Restart = "no");

var pgLoader = builder.AddDockerfile("borealis-pgloader", "docker/pgloader")
    .WithReference(postgresdb, "PostgresDb")
    .WithBindMount(source: "./docker/pgloader/db", "/sqlite/db")
    .WaitFor(postgresdb)
    .WaitForCompletion(migrations)
    .PublishAsDockerComposeService((_, service) => service.Restart = "no");

var whiteoutBotService = builder.AddExternalService("whiteout-bot", new Uri("http://gift-code-api.whiteout-bot.com/"));
var destructoidService = builder.AddExternalService("destructoid", new Uri("https://www.destructoid.com/"));
var wosgiftcodesService = builder.AddExternalService("wosgiftcodes", new Uri("https://wosgiftcodes.com/"));
var wosrewardsService = builder.AddExternalService("wosrewards", new Uri("https://wosrewards.com/"));
var wosGiftcodeApiService = builder.AddExternalService("wos-giftcode-api", new Uri("https://wos-giftcode-api.centurygame.com/"));

var web = builder
    .AddProject<Borealis_Web>("borealis-web")
    .WithExternalHttpEndpoints()
    .WithEnvironment("Discord__BotToken", discordBotToken)
    .WithEnvironment("Discord__ClientSecret", discordClientSecret)
    .WithEnvironment("Discord__ClientId", discordClientId)
    .WithEnvironment("Borealis__ApplicationUrl", borealisApplicationUrl)
    .WithEnvironment("WhiteoutSurvivalProxy__FunctionKey", whiteoutSurvivalProxyFunctionKey)
    .WithReference(postgresdb, "PostgresDb")
    .WithReference(whiteoutBotService)
    .WithReference(destructoidService)
    .WithReference(wosgiftcodesService)
    .WithReference(wosrewardsService)
    .WithReference(wosGiftcodeApiService)
    .WaitForCompletion(migrations)
    .WaitFor(postgresdb)
    .PublishAsDockerFile(options => {
        options.WithDockerfile("../.."); // This can't have an ending slash
    })
    .PublishAsDockerComposeService((_, service) => {
        service.Restart = "unless-stopped";
    });

if(builder.ExecutionContext.IsRunMode) {
    var proxyProject = builder.AddAzureFunctionsProject<Borealis_WhiteoutSurvivalProxy>("borealis-wos-proxy");
    web.WithReference(proxyProject);
} else {
    var proxyUrl = builder.AddParameter("borealis-wos-proxy-url");
    var proxyProject = builder.AddExternalService("borealis-wos-proxy", proxyUrl);
}

if(builder.ExecutionContext.IsRunMode) {
    var frontend = builder.AddNpmApp("frontend", "../Borealis.Frontend", "dev");

    web.WaitFor(frontend);
}

builder.Build().Run();
