using DroneBuilder.Infrastructure;
using DroneBuilder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.API.Extensions;

public static class DatabaseExtension
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        bool applyMigrations = app.Configuration.GetValue<bool>("DatabaseInitialization:ApplyMigrations");
        bool seedIdentity = app.Configuration.GetValue<bool>("DatabaseInitialization:SeedIdentity");

        if (!applyMigrations && !seedIdentity)
        {
            return;
        }

        using IServiceScope scope = app.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;

        if (applyMigrations)
        {
            ApplicationDbContext dbContext = services.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        if (seedIdentity)
        {
            await IdentitySeeder.SeedRolesAndUsersAsync(services, app.Configuration);
        }
    }
}
