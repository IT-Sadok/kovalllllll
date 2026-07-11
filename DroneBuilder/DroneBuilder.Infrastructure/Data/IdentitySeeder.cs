using DroneBuilder.Domain.Constants;
using DroneBuilder.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DroneBuilder.Infrastructure.Data;

public static class IdentitySeeder
{
    public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
    {
        RoleManager<IdentityRole<Guid>> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        UserManager<User> userManager = serviceProvider.GetRequiredService<UserManager<User>>();

        string[] roles = [RoleNames.Admin, RoleNames.User];
        foreach (string role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
            }
        }

        const string adminEmail = "admin@dronebuilder.com";
        User? adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            IdentityResult result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
            }
        }

        const string defaultUserEmail = "user@dronebuilder.com";
        User? defaultUser = await userManager.FindByEmailAsync(defaultUserEmail);

        if (defaultUser == null)
        {
            defaultUser = new User
            {
                UserName = defaultUserEmail,
                Email = defaultUserEmail,
                EmailConfirmed = true
            };

            IdentityResult result = await userManager.CreateAsync(defaultUser, "User123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(defaultUser, RoleNames.User);
            }
        }
    }
}
