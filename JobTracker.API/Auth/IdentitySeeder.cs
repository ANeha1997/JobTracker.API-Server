using JobTracker.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace JobTracker.API.Auth
{
    public static class IdentitySeeder
    {
        public static void Seed(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1) Ensure roles exist
            string[] roles = { AppRoles.User, AppRoles.Admin };

            foreach (var roleName in roles)
            {
                var roleExists = roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult();
                if (!roleExists)
                {
                    roleManager.CreateAsync(new IdentityRole(roleName))
                               .GetAwaiter().GetResult();
                }
            }

            // 2) Ensure a default admin user
            const string adminEmail = "admin@jobtracker.local";
            const string adminUserName = "admin";
            const string adminPassword = "Admin123$";  

            var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminEmail
                };

                var result = userManager.CreateAsync(adminUser, adminPassword)
                                        .GetAwaiter().GetResult();

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(adminUser, AppRoles.Admin)
                               .GetAwaiter().GetResult();
                }
            }
        }
    }
}
