using DroneBuilder.Domain.Entities;
using DroneBuilder.Domain.Entities.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DroneBuilder.Infrastructure;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        builder.Entity<Warehouse>().HasData(new Warehouse
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "Main Warehouse",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }

    public DbSet<Message> Messages { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<ProductAttribute> ProductAttributes { get; set; }
    public DbSet<ProductGroup> ProductGroups { get; set; }
    public DbSet<ImportRun> ImportRuns { get; set; }
    public DbSet<ComponentSpec> ComponentSpecs { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Build> Builds { get; set; }
    public DbSet<BuildItem> BuildItems { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<WarehouseItem> WarehouseItems { get; set; }
}
