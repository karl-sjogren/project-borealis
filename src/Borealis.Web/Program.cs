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

builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<BorealisContext>();

builder.Services.AddControllersWithViews();

builder.Services.ConfigureApplicationCookie(options => {
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);

    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

builder.Services.Configure<WhiteoutSurvivalOptions>(builder.Configuration.GetSection("WhiteoutSurvival"));

builder.Services.AddHttpClient<IWhiteoutSurvivalHttpClient, WhiteoutSurvivalHttpClient>()
    .ConfigureHttpClient((serviceProvider, client) => {
        var options = serviceProvider.GetRequiredService<IOptions<WhiteoutSurvivalOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseUrl);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("Origin", options.OriginUrl);
    });

builder.Services.AddAuthentication()
        .AddDiscord(options => {
            options.ClientId = builder.Configuration["DiscordClientId"] ?? throw new InvalidOperationException("DiscordClientId is not set in the configuration.");
            options.ClientSecret = builder.Configuration["DiscordClientSecret"] ?? throw new InvalidOperationException("DiscordClientSecret is not set in the configuration.");
        });

builder.Services.AddScoped<IPlayerService, PlayerService>();

var app = builder.Build();

// Apply migrations
using(var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<BorealisContext>();
    db.Database.Migrate();
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
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
