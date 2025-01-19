using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using Borealis.Core;
using Borealis.Core.Contracts;
using Borealis.Core.HttpClients;
using Borealis.Core.Options;
using Borealis.Core.Services;
using Borealis.Web.HostedServices;
using Borealis.Web.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Polly;
using Polly.Extensions.Http;
using Shorthand.Vite;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.ConfigureApplicationCookie(options => {
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;

    options.LoginPath = "/";
    options.AccessDeniedPath = "/";
    options.SlidingExpiration = true;
});

builder.Services.Configure<WhiteoutSurvivalOptions>(builder.Configuration.GetSection("WhiteoutSurvival"));
builder.Services.Configure<BorealisAuthenticationOptions>(builder.Configuration.GetSection("BorealisAuthentication"));

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() {
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

builder.Services.AddHttpClient<IWhiteoutSurvivalHttpClient, WhiteoutSurvivalHttpClient>()
    .ConfigureHttpClient((serviceProvider, client) => {
        var options = serviceProvider.GetRequiredService<IOptions<WhiteoutSurvivalOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseUrl);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("Origin", options.OriginUrl);
    })
    .AddPolicyHandler(GetRetryPolicy());

builder.Services
    .AddAuthentication(options => {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
    })
        .AddCookie()
        .AddDiscord(options => {
            options.ClientId = builder.Configuration["DiscordClientId"] ?? throw new InvalidOperationException("DiscordClientId is not set in the configuration.");
            options.ClientSecret = builder.Configuration["DiscordClientSecret"] ?? throw new InvalidOperationException("DiscordClientSecret is not set in the configuration.");

            options.ClaimActions.MapCustomJson(ClaimTypes.Name, user => user.GetString("global_name"));
            options.ClaimActions.MapCustomJson("urn:discord:avatar:url", user =>
                string.Format(
                    CultureInfo.InvariantCulture,
                    "https://cdn.discordapp.com/avatars/{0}/{1}.{2}",
                    user.GetString("id"),
                    user.GetString("avatar"),
                    user.GetString("avatar")?.StartsWith("a_", StringComparison.Ordinal) == true ? "gif" : "png"));

            options.Events.OnCreatingTicket = async (ctx) => {
                var options = ctx.HttpContext.RequestServices.GetRequiredService<IOptionsSnapshot<BorealisAuthenticationOptions>>();

                var userIdentity = ctx.Principal?.Identity as ClaimsIdentity;

                if(userIdentity == null) {
                    return;
                }

                var externalId = userIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if(string.IsNullOrWhiteSpace(externalId)) {
                    return;
                }

                var borealisContext = ctx.HttpContext.RequestServices.GetRequiredService<BorealisContext>();

                var user = await borealisContext.Users.FirstOrDefaultAsync(x => x.ExternalId == externalId);

                if(user?.IsLockedOut != false) {
                    return;
                }

                userIdentity.RemoveClaim(userIdentity.FindFirst(ClaimTypes.Name));
                userIdentity.AddClaim(new Claim(ClaimTypes.Name, user.Name));

                if(user.IsApproved) {
                    userIdentity.AddClaim(new Claim(ClaimTypes.Role, "TrustedUser"));
                } else {
                    userIdentity.AddClaim(new Claim(ClaimTypes.Role, "PendingApproval"));
                }

                if(user.IsAdmin) {
                    userIdentity.AddClaim(new Claim(ClaimTypes.Role, "AdminUser"));
                }
            };
        });

builder.Services.AddVite(options => {
    options.ManifestFileName = ".vite/manifest.json";
    options.Port = 5010;
    options.Https = true;
});

builder.Services.AddScoped<IGiftCodeService, GiftCodeService>();
builder.Services.AddScoped<IMessageTemplateService, MessageTemplateService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddSingleton<IGiftCodeRedemptionQueue, GiftCodeRedemptionQueue>();
builder.Services.AddHostedService<GiftCodeRedemptionQueueProcessingHostedService>();
builder.Services.AddHostedService<UpdatePlayersHostedService>();

var app = builder.Build();

// Apply migrations
using(var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<BorealisContext>();
    await db.Database.MigrateAsync();
}

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
