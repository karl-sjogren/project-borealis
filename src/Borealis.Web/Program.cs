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
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Serilog;
using Shorthand.Vite;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/borealis-web.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog();
builder.Services.AddSingleton(TimeProvider.System);

// Add services to the container.
var connectionStringBuilder = new SqliteConnectionStringBuilder {
    Mode = SqliteOpenMode.ReadWriteCreate,
    DataSource = builder.Configuration["SqlitePath"]
};

builder.Services.AddDbContext<BorealisContext>(options =>
    options.UseSqlite(connectionStringBuilder.ConnectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddMvc(options => {
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
});

if(builder.Environment.IsProduction()) {
    builder.Services.AddLettuceEncrypt();
}

builder.Services.Configure<WhiteoutSurvivalOptions>(builder.Configuration.GetSection("WhiteoutSurvival"));
builder.Services.Configure<BorealisAuthenticationOptions>(builder.Configuration.GetSection("BorealisAuthentication"));

builder.Services.AddHttpClients();

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

builder.Services.AddScoped<IGiftCodeScanner, DestructoidGiftCodeScanner>();
builder.Services.AddScoped<IGiftCodeScanner, WosRewardsGiftCodeScanner>();

builder.Services.AddSingleton<IGiftCodeRedemptionQueue, GiftCodeRedemptionQueue>();
builder.Services.AddHostedService<ScanForGiftCodesHostedService>();
builder.Services.AddHostedService<GiftCodeCheckDailyHostedService>();
builder.Services.AddHostedService<GiftCodeRedemptionQueueProcessingHostedService>();
builder.Services.AddHostedService<UpdatePlayersHostedService>();

builder.Services.AddScoped<IDiscordBotService, DiscordBotService>();
builder.Services.AddSingleton<IDiscordClient, DiscordSocketClient>();
builder.Services.AddHostedService<DiscordBotInitializationService>();

var app = builder.Build();

// Apply migrations
using(var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<BorealisContext>();
    await db.Database.MigrateAsync();
}

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment()) {
    app.UseMigrationsEndPoint();
} else {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

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
