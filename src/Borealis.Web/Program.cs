using Borealis.Core;
using Borealis.Core.Contracts;
using Borealis.Core.GiftCodeScanners;
using Borealis.Core.Options;
using Borealis.Core.Services;
using Borealis.Web.Extensions;
using Borealis.Web.HostedServices;
using Borealis.Web.Mvc;
using Borealis.WhiteoutSurvivalHttpClient;
using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Serilog;
using Shorthand.Vite;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/borealis-web.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddSerilog();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddOptions<CapSolverOptions>().Bind(builder.Configuration.GetSection("CapSolver")).ValidateOnStart();
builder.Services.AddOptions<WosLandOptions>().Bind(builder.Configuration.GetSection("WosLand")).ValidateOnStart();
builder.Services.AddOptions<WhiteoutSurvivalOptions>().Bind(builder.Configuration.GetSection("WhiteoutSurvival")).ValidateOnStart();

builder.Services.AddOptions<BorealisOptions>().Bind(builder.Configuration.GetSection("Borealis")).ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<BorealisOptions>, BorealisOptionsValidator>();

builder.AddNpgsqlDbContext<BorealisContext>("PostgresDb");

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddMvc(options => {
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
});

var enableLettuceEncrypt = builder.Configuration.GetValue("LettuceEncrypt:Enabled", false);
if(builder.Environment.IsProduction() && enableLettuceEncrypt) {
    builder.Services.AddLettuceEncrypt();
}

builder.Services.AddHttpClients();
builder.Services.AddCaptchaServices();

builder.Services.AddAppAuthentication(builder.Configuration);

builder.Services.AddVite(options => {
    options.ManifestFileName = ".vite/manifest.json";
    options.Port = 5010;
    options.Https = true;
});

builder.Services.AddScoped<IGiftCodeService, GiftCodeService>();
builder.Services.AddScoped<IMessageTemplateService, MessageTemplateService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWhiteoutSurvivalService, WhiteoutSurvivalService>();

builder.Services.AddScoped<IGiftCodeScanner, DestructoidGiftCodeScanner>();
builder.Services.AddScoped<IGiftCodeScanner, WosGiftCodesGiftCodeScanner>();
builder.Services.AddScoped<IGiftCodeScanner, WosLandGiftCodeScanner>();
builder.Services.AddScoped<IGiftCodeScanner, WosRewardsGiftCodeScanner>();

builder.Services.AddSingleton<IGiftCodeRedemptionQueue, GiftCodeRedemptionQueue>();
if(builder.Environment.IsProduction()) {
    builder.Services.AddHostedService<GiftCodeRedemptionQueueProcessingHostedService>();
    builder.Services.AddHostedService<GiftCodeCheckDailyHostedService>();
    builder.Services.AddHostedService<ScanForGiftCodesHostedService>();
    builder.Services.AddHostedService<UpdatePlayersHostedService>();
}

builder.Services.AddScoped<IDiscordBotService, DiscordBotService>();
builder.Services.AddSingleton<IDiscordClient>(_ => {
    var discordClient = new DiscordSocketClient(new DiscordSocketConfig {
        GatewayIntents = GatewayIntents.AllUnprivileged & ~GatewayIntents.GuildScheduledEvents & ~GatewayIntents.GuildInvites
    });

    return discordClient;
});
builder.Services.AddHostedService<DiscordBotInitializationService>();

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment()) {
    app.UseMigrationsEndPoint();
} else {
    app.UseExceptionHandler("/Error");
    app.UseForwardedHeaders();
    app.UseHsts();
}

var useHttpsRedirection = builder.Configuration.GetValue("Borealis:UseHttpRedirection", false);
if(useHttpsRedirection) {
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

static void onPrepareResponse(StaticFileResponseContext ctx) {
    string[] _staticAssetPaths = ["/assets"];

    var requestPath = ctx.Context.Request.Path;
    if(!_staticAssetPaths.Any(path => requestPath.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase))) {
        return;
    }

    var cacheDuration = TimeSpan.FromDays(365).TotalSeconds;
    ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,must-revalidate,max-age=" + cacheDuration + ",immutable";
}

app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = onPrepareResponse });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages()
   .WithStaticAssets();

await app.RunAsync();
