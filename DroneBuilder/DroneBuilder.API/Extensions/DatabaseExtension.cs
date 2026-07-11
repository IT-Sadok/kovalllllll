using DroneBuilder.Infrastructure;
using DroneBuilder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.API.Extensions;

public static class DatabaseExtension
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;

        ApplicationDbContext dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        await IdentitySeeder.SeedRolesAndUsersAsync(services);
    }
}
