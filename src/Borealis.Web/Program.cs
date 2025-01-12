using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Borealis.Core;
using Borealis.Core.HttpClients;
using Microsoft.Extensions.Options;
using Borealis.Core.Options;
using System.Net.Http.Headers;
using Borealis.Core.Contracts;
using Borealis.Core.Services;
using Shorthand.Vite;
using Polly;
using Polly.Extensions.Http;
using Borealis.Web.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Authentication;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Borealis.Web.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);

// Add services to the container.
var connectionStringBuilder = new SqlConnectionStringBuilder {
    DataSource = builder.Configuration["DatabaseHost"],
    InitialCatalog = builder.Configuration["DatabaseName"],
    IntegratedSecurity = false,
    UserID = builder.Configuration["DatabaseUser"],
    Password = builder.Configuration["DatabasePassword"],
    TrustServerCertificate = true
};

builder.Services.AddDbContext<BorealisContext>(options =>
    options.UseSqlServer(connectionStringBuilder.ConnectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;

    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<BorealisContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.AddMvc(options => {
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));

    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireRole("TrustedUser")
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.ConfigureApplicationCookie(options => {
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);

    options.LoginPath = "/";
    options.AccessDeniedPath = "/";
    options.SlidingExpiration = true;
});

builder.Services.Configure<WhiteoutSurvivalOptions>(builder.Configuration.GetSection("WhiteoutSurvival"));

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

builder.Services.AddAuthentication()
        .AddDiscord(options => {
            options.ClientId = builder.Configuration["DiscordClientId"] ?? throw new InvalidOperationException("DiscordClientId is not set in the configuration.");
            options.ClientSecret = builder.Configuration["DiscordClientSecret"] ?? throw new InvalidOperationException("DiscordClientSecret is not set in the configuration.");

            options.ClaimActions.MapCustomJson("urn:discord:avatar:url", user =>
                string.Format(
                    CultureInfo.InvariantCulture,
                    "https://cdn.discordapp.com/avatars/{0}/{1}.{2}",
                    user.GetString("id"),
                    user.GetString("avatar"),
                    user.GetString("avatar")?.StartsWith("a_", StringComparison.Ordinal) == true ? "gif" : "png"));
        });

builder.Services.AddVite(options => {
    options.ManifestFileName = ".vite/manifest.json";
    options.Port = 5010;
    options.Https = true;
});

builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IGiftCodeService, GiftCodeService>();

builder.Services.AddSingleton<IGiftCodeRedemptionQueue, GiftCodeRedemptionQueue>();

var app = builder.Build();

// Apply migrations
using(var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<BorealisContext>();
    await db.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await AuthenticationSeeder.SeedRolesAsync(roleManager);

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    await AuthenticationSeeder.SeedUserRolesAsync(userManager);
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

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages()
   .WithStaticAssets();

await app.RunAsync();
