using System.Globalization;
using System.Security.Claims;
using AspNet.Security.OAuth.Discord;
using Borealis.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Borealis.Web.Extensions;

public static partial class IServiceCollectionExtensions {
    public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration configuration) {
        services.ConfigureApplicationCookie(options => {
            // Cookie settings
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;

            options.LoginPath = "/";
            options.AccessDeniedPath = "/";
        });

        services
            .AddAuthentication(options => {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options => {
                options.Cookie.IsEssential = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(48);
                options.SlidingExpiration = true;
            })
            .AddDiscord(options => {
                options.ClientId = configuration["DiscordClientId"] ?? throw new InvalidOperationException("DiscordClientId is not set in the configuration.");
                options.ClientSecret = configuration["DiscordClientSecret"] ?? throw new InvalidOperationException("DiscordClientSecret is not set in the configuration.");

                options.ClaimActions.MapCustomJson(ClaimTypes.Name, user => user.GetString("global_name"));
                options.ClaimActions.MapCustomJson("urn:discord:avatar:url", user =>
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "https://cdn.discordapp.com/avatars/{0}/{1}.{2}",
                        user.GetString("id"),
                        user.GetString("avatar"),
                        user.GetString("avatar")?.StartsWith("a_", StringComparison.Ordinal) == true ? "gif" : "png"));

                options.Events.OnCreatingTicket = async (ctx) => {
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

                    ctx.Properties.IsPersistent = true;
                };
            });

        return services;
    }
}
