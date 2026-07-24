using DroneBuilder.Domain.Constants;
using DroneBuilder.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DroneBuilder.Infrastructure.Data;

public static class IdentitySeeder
{
    public static async Task SeedRolesAndUsersAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
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

        await SeedUserAsync(userManager, configuration, "IdentitySeed:Admin", RoleNames.Admin);
        await SeedUserAsync(userManager, configuration, "IdentitySeed:User", RoleNames.User);
    }

    private static async Task SeedUserAsync(
        UserManager<User> userManager,
        IConfiguration configuration,
        string sectionName,
        string role)
    {
        string? email = configuration[$"{sectionName}:Email"];
        string? password = configuration[$"{sectionName}:Password"];

        bool hasEmail = !string.IsNullOrWhiteSpace(email);
        bool hasPassword = !string.IsNullOrWhiteSpace(password);

        if (!hasEmail && !hasPassword)
        {
            return;
        }

        if (!hasEmail || !hasPassword)
        {
            throw new InvalidOperationException(
                $"{sectionName} must define both Email and Password when identity seeding is enabled.");
        }

        User? user = await userManager.FindByEmailAsync(email!);
        if (user != null)
        {
            return;
        }

        user = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        IdentityResult result = await userManager.CreateAsync(user, password!);
        if (!result.Succeeded)
        {
            string errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed to seed {sectionName}: {errors}");
        }

        await userManager.AddToRoleAsync(user, role);
    }
}
