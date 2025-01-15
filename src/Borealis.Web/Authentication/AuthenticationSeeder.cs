using Microsoft.AspNetCore.Identity;

namespace Borealis.Web.Authentication;

public static class AuthenticationSeeder {
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager) {
        if(!await roleManager.RoleExistsAsync("TrustedUser")) {
            await roleManager.CreateAsync(new IdentityRole("TrustedUser"));
        }

        if(!await roleManager.RoleExistsAsync("AdminUser")) {
            await roleManager.CreateAsync(new IdentityRole("AdminUser"));
        }
    }

    public static async Task SeedUserRolesAsync(UserManager<IdentityUser> userManager) {
        var userCount = userManager.Users.Count();
        if(userCount == 1) {
            var user = userManager.Users.First();
            if(!await userManager.IsInRoleAsync(user, "TrustedUser")) {
                await userManager.AddToRoleAsync(user, "TrustedUser");
            }

            if(!await userManager.IsInRoleAsync(user, "AdminUser")) {
                await userManager.AddToRoleAsync(user, "AdminUser");
            }
        }
    }
}
