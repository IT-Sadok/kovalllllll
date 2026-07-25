using DroneBuilder.Domain.Entities;
using DroneBuilder.Infrastructure.Data.Entities;
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
            Code = "main",
            Name = "Main Warehouse",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        builder.Entity<UnitDefinition>().HasData(
            CreateUnit("00000000-0000-0000-0001-000000000001", "millimeter", "Millimeter", "mm", "length", 1m),
            CreateUnit("00000000-0000-0000-0001-000000000002", "inch", "Inch", "in", "length", 25.4m),
            CreateUnit("00000000-0000-0000-0001-000000000003", "volt", "Volt", "V", "voltage", 1m),
            CreateUnit("00000000-0000-0000-0001-000000000004", "ampere", "Ampere", "A", "current", 1m),
            CreateUnit("00000000-0000-0000-0001-000000000005", "cell", "Battery cell count", "S", "cell_count", 1m),
            CreateUnit("00000000-0000-0000-0001-000000000006", "gram", "Gram", "g", "mass", 1m),
            CreateUnit("00000000-0000-0000-0001-000000000007", "kilogram", "Kilogram", "kg", "mass", 1000m),
            CreateUnit("00000000-0000-0000-0001-000000000008", "watt", "Watt", "W", "power", 1m),
            CreateUnit("00000000-0000-0000-0001-000000000009", "milliampere-hour", "Milliampere-hour", "mAh", "electric_charge", 1m),
            CreateUnit("00000000-0000-0000-0001-000000000010", "square-millimeter", "Square millimeter", "mm²", "area", 1m),
            CreateUnit("00000000-0000-0000-0001-000000000011", "hertz", "Hertz", "Hz", "frequency", 1m),
            CreateUnit("00000000-0000-0000-0001-000000000012", "megahertz", "Megahertz", "MHz", "frequency", 1000000m),
            CreateUnit("00000000-0000-0000-0001-000000000013", "gigahertz", "Gigahertz", "GHz", "frequency", 1000000000m),
            CreateUnit("00000000-0000-0000-0001-000000000014", "rpm", "Revolutions per minute", "RPM", "rotational_speed", 1m),
            CreateUnit("00000000-0000-0000-0001-000000000015", "kv", "Motor velocity constant", "KV", "motor_velocity_constant", 1m),
            CreateUnit("00000000-0000-0000-0001-000000000016", "celsius", "Degree Celsius", "°C", "temperature", 1m));

        SeedUnitAliases(builder);
        SeedComponentMetadata(builder);
    }

    public DbSet<OutboxMessage> Messages => Set<OutboxMessage>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ComponentType> ComponentTypes => Set<ComponentType>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ComponentTypeProperty> ComponentTypeProperties => Set<ComponentTypeProperty>();
    public DbSet<Image> Images => Set<Image>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Value> Values => Set<Value>();
    public DbSet<PropertyValue> PropertyValues => Set<PropertyValue>();
    public DbSet<UnitDefinition> UnitDefinitions => Set<UnitDefinition>();
    public DbSet<ProductPropertyValue> ProductPropertyValues => Set<ProductPropertyValue>();
    public DbSet<ProductVariantPropertyValue> ProductVariantPropertyValues => Set<ProductVariantPropertyValue>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseItem> WarehouseItems => Set<WarehouseItem>();
    public DbSet<InventoryReservation> InventoryReservations => Set<InventoryReservation>();
    public DbSet<ImportSource> ImportSources => Set<ImportSource>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ImportItem> ImportItems => Set<ImportItem>();
    public DbSet<ProductExternalReference> ProductExternalReferences => Set<ProductExternalReference>();
    public DbSet<ProductVariantExternalReference> ProductVariantExternalReferences
        => Set<ProductVariantExternalReference>();
    public DbSet<UnitAlias> UnitAliases => Set<UnitAlias>();
    public DbSet<PropertyAlias> PropertyAliases => Set<PropertyAlias>();
    public DbSet<ValueAlias> ValueAliases => Set<ValueAlias>();

    public override int SaveChanges()
    {
        ValidateDomainInvariants();
        ApplyAuditTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ValidateDomainInvariants();
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ValidateDomainInvariants()
    {
        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Property> entry
                 in ChangeTracker.Entries<Property>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            entry.Entity.ValidateDefinition();
        }

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ProductPropertyValue> entry
                 in ChangeTracker.Entries<ProductPropertyValue>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            entry.Entity.Validate();
        }

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ProductVariantPropertyValue> entry
                 in ChangeTracker.Entries<ProductVariantPropertyValue>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            entry.Entity.Validate();
        }

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<UnitAlias> entry
                 in ChangeTracker.Entries<UnitAlias>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            entry.Entity.Normalize();
        }

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PropertyAlias> entry
                 in ChangeTracker.Entries<PropertyAlias>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            entry.Entity.Normalize();
        }

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ValueAlias> entry
                 in ChangeTracker.Entries<ValueAlias>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            entry.Entity.Normalize();
        }

        IEnumerable<Product> directlyChangedProducts = ChangeTracker.Entries<Product>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
            .Select(entry => entry.Entity);

        IEnumerable<Product> productsWithChangedVariants = ChangeTracker.Entries<ProductVariant>()
            .Where(entry => entry.State is
                EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entry => entry.Entity.Product
                ?? throw new InvalidOperationException(
                    "Changed product variants must include their Product navigation."));

        foreach (Product product in directlyChangedProducts
                     .Concat(productsWithChangedVariants)
                     .DistinctBy(product => product.Id))
        {
            product.ValidateForPublication();
        }
    }

    private void ApplyAuditTimestamps()
    {
        DateTime now = DateTime.UtcNow;
        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<AuditableEntity> entry
                 in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }

    private static UnitDefinition CreateUnit(
        string id,
        string code,
        string name,
        string symbol,
        string dimension,
        decimal conversionFactor)
    {
        var timestamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return new UnitDefinition
        {
            Id = Guid.Parse(id),
            Code = code,
            Name = name,
            Symbol = symbol,
            Dimension = dimension,
            ConversionFactorToBase = conversionFactor,
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        };
    }

    private static void SeedUnitAliases(ModelBuilder builder)
    {
        (string Id, string UnitId, string Alias, string Normalized)[] aliases =
        [
            ("00000000-0000-0000-0005-000000000001", "00000000-0000-0000-0001-000000000001", "mm", "mm"),
            ("00000000-0000-0000-0005-000000000002", "00000000-0000-0000-0001-000000000001", "millimeter", "millimeter"),
            ("00000000-0000-0000-0005-000000000003", "00000000-0000-0000-0001-000000000002", "in", "in"),
            ("00000000-0000-0000-0005-000000000004", "00000000-0000-0000-0001-000000000002", "inch", "inch"),
            ("00000000-0000-0000-0005-000000000005", "00000000-0000-0000-0001-000000000002", "\"", "\""),
            ("00000000-0000-0000-0005-000000000006", "00000000-0000-0000-0001-000000000002", "″", "″"),
            ("00000000-0000-0000-0005-000000000007", "00000000-0000-0000-0001-000000000003", "V", "v"),
            ("00000000-0000-0000-0005-000000000008", "00000000-0000-0000-0001-000000000003", "volt", "volt"),
            ("00000000-0000-0000-0005-000000000009", "00000000-0000-0000-0001-000000000004", "A", "a"),
            ("00000000-0000-0000-0005-000000000010", "00000000-0000-0000-0001-000000000004", "amp", "amp"),
            ("00000000-0000-0000-0005-000000000011", "00000000-0000-0000-0001-000000000005", "S", "s"),
            ("00000000-0000-0000-0005-000000000012", "00000000-0000-0000-0001-000000000005", "cell", "cell"),
            ("00000000-0000-0000-0005-000000000013", "00000000-0000-0000-0001-000000000006", "g", "g"),
            ("00000000-0000-0000-0005-000000000014", "00000000-0000-0000-0001-000000000007", "kg", "kg"),
            ("00000000-0000-0000-0005-000000000015", "00000000-0000-0000-0001-000000000008", "W", "w"),
            ("00000000-0000-0000-0005-000000000016", "00000000-0000-0000-0001-000000000009", "mAh", "mah"),
            ("00000000-0000-0000-0005-000000000017", "00000000-0000-0000-0001-000000000010", "mm2", "mm2"),
            ("00000000-0000-0000-0005-000000000018", "00000000-0000-0000-0001-000000000010", "mm²", "mm²"),
            ("00000000-0000-0000-0005-000000000019", "00000000-0000-0000-0001-000000000011", "Hz", "hz"),
            ("00000000-0000-0000-0005-000000000020", "00000000-0000-0000-0001-000000000012", "MHz", "mhz"),
            ("00000000-0000-0000-0005-000000000021", "00000000-0000-0000-0001-000000000013", "GHz", "ghz"),
            ("00000000-0000-0000-0005-000000000022", "00000000-0000-0000-0001-000000000014", "RPM", "rpm"),
            ("00000000-0000-0000-0005-000000000023", "00000000-0000-0000-0001-000000000015", "KV", "kv"),
            ("00000000-0000-0000-0005-000000000024", "00000000-0000-0000-0001-000000000016", "°C", "°c"),
            ("00000000-0000-0000-0005-000000000025", "00000000-0000-0000-0001-000000000001", "millimeters", "millimeters"),
            ("00000000-0000-0000-0005-000000000026", "00000000-0000-0000-0001-000000000001", "millimetre", "millimetre"),
            ("00000000-0000-0000-0005-000000000027", "00000000-0000-0000-0001-000000000001", "millimetres", "millimetres"),
            ("00000000-0000-0000-0005-000000000028", "00000000-0000-0000-0001-000000000002", "inches", "inches"),
            ("00000000-0000-0000-0005-000000000029", "00000000-0000-0000-0001-000000000003", "volts", "volts"),
            ("00000000-0000-0000-0005-000000000030", "00000000-0000-0000-0001-000000000004", "amps", "amps"),
            ("00000000-0000-0000-0005-000000000031", "00000000-0000-0000-0001-000000000004", "ampere", "ampere"),
            ("00000000-0000-0000-0005-000000000032", "00000000-0000-0000-0001-000000000005", "cells", "cells"),
            ("00000000-0000-0000-0005-000000000033", "00000000-0000-0000-0001-000000000006", "grams", "grams"),
            ("00000000-0000-0000-0005-000000000034", "00000000-0000-0000-0001-000000000008", "watts", "watts"),
            ("00000000-0000-0000-0005-000000000035", "00000000-0000-0000-0001-000000000016", "C", "c"),
            ("00000000-0000-0000-0005-000000000036", "00000000-0000-0000-0001-000000000016", "celsius", "celsius")
        ];

        DateTime timestamp = SeedTimestamp();
        builder.Entity<UnitAlias>().HasData(aliases.Select(alias => new UnitAlias
        {
            Id = Guid.Parse(alias.Id),
            UnitDefinitionId = Guid.Parse(alias.UnitId),
            Alias = alias.Alias,
            NormalizedAlias = alias.Normalized,
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        }));
    }

    private static void SeedComponentMetadata(ModelBuilder builder)
    {
        DateTime timestamp = SeedTimestamp();

        ComponentType[] componentTypes =
        [
            CreateComponentType("00000000-0000-0000-0002-000000000001", "motor", "Motor"),
            CreateComponentType("00000000-0000-0000-0002-000000000002", "frame", "Frame"),
            CreateComponentType("00000000-0000-0000-0002-000000000003", "battery", "Battery"),
            CreateComponentType("00000000-0000-0000-0002-000000000004", "esc", "Electronic Speed Controller"),
            CreateComponentType("00000000-0000-0000-0002-000000000005", "flight-controller", "Flight Controller")
        ];
        builder.Entity<ComponentType>().HasData(componentTypes);

        Property[] properties =
        [
            CreateProperty("00000000-0000-0000-0003-000000000001", "motor-kv", "Motor KV", SpecificationDataType.Number, "00000000-0000-0000-0001-000000000015"),
            CreateProperty("00000000-0000-0000-0003-000000000002", "max-current", "Maximum Current", SpecificationDataType.Number, "00000000-0000-0000-0001-000000000004"),
            CreateProperty("00000000-0000-0000-0003-000000000003", "input-voltage", "Input Voltage", SpecificationDataType.NumericRange, "00000000-0000-0000-0001-000000000003"),
            CreateProperty("00000000-0000-0000-0003-000000000004", "shaft-diameter", "Shaft Diameter", SpecificationDataType.Number, "00000000-0000-0000-0001-000000000001"),
            CreateProperty("00000000-0000-0000-0003-000000000005", "propeller-size", "Propeller Size", SpecificationDataType.Number, "00000000-0000-0000-0001-000000000002"),
            CreateProperty("00000000-0000-0000-0003-000000000006", "wheelbase", "Wheelbase", SpecificationDataType.Number, "00000000-0000-0000-0001-000000000001"),
            CreateProperty("00000000-0000-0000-0003-000000000007", "mount-width", "Mount Width", SpecificationDataType.Number, "00000000-0000-0000-0001-000000000001"),
            CreateProperty("00000000-0000-0000-0003-000000000008", "mount-height", "Mount Height", SpecificationDataType.Number, "00000000-0000-0000-0001-000000000001"),
            CreateProperty("00000000-0000-0000-0003-000000000009", "battery-cell-count", "Battery Cell Count", SpecificationDataType.Number, "00000000-0000-0000-0001-000000000005"),
            CreateProperty("00000000-0000-0000-0003-000000000010", "battery-capacity", "Battery Capacity", SpecificationDataType.Number, "00000000-0000-0000-0001-000000000009"),
            CreateProperty("00000000-0000-0000-0003-000000000011", "connector-type", "Connector Type", SpecificationDataType.Option, null)
        ];
        builder.Entity<Property>().HasData(properties);
        SeedPropertyAliases(builder);

        (string ComponentTypeId, string PropertyId, bool Required, bool Variant, int SortOrder)[] links =
        [
            ("00000000-0000-0000-0002-000000000001", "00000000-0000-0000-0003-000000000001", true, true, 10),
            ("00000000-0000-0000-0002-000000000001", "00000000-0000-0000-0003-000000000002", false, true, 20),
            ("00000000-0000-0000-0002-000000000001", "00000000-0000-0000-0003-000000000003", true, true, 30),
            ("00000000-0000-0000-0002-000000000001", "00000000-0000-0000-0003-000000000004", false, false, 40),
            ("00000000-0000-0000-0002-000000000002", "00000000-0000-0000-0003-000000000005", true, false, 10),
            ("00000000-0000-0000-0002-000000000002", "00000000-0000-0000-0003-000000000006", false, false, 20),
            ("00000000-0000-0000-0002-000000000002", "00000000-0000-0000-0003-000000000007", true, false, 30),
            ("00000000-0000-0000-0002-000000000002", "00000000-0000-0000-0003-000000000008", true, false, 40),
            ("00000000-0000-0000-0002-000000000003", "00000000-0000-0000-0003-000000000009", true, true, 10),
            ("00000000-0000-0000-0002-000000000003", "00000000-0000-0000-0003-000000000010", true, true, 20),
            ("00000000-0000-0000-0002-000000000003", "00000000-0000-0000-0003-000000000011", true, true, 30),
            ("00000000-0000-0000-0002-000000000003", "00000000-0000-0000-0003-000000000002", false, true, 40),
            ("00000000-0000-0000-0002-000000000004", "00000000-0000-0000-0003-000000000002", true, true, 10),
            ("00000000-0000-0000-0002-000000000004", "00000000-0000-0000-0003-000000000003", true, true, 20),
            ("00000000-0000-0000-0002-000000000004", "00000000-0000-0000-0003-000000000007", false, false, 30),
            ("00000000-0000-0000-0002-000000000004", "00000000-0000-0000-0003-000000000008", false, false, 40),
            ("00000000-0000-0000-0002-000000000005", "00000000-0000-0000-0003-000000000003", true, false, 10),
            ("00000000-0000-0000-0002-000000000005", "00000000-0000-0000-0003-000000000007", true, false, 20),
            ("00000000-0000-0000-0002-000000000005", "00000000-0000-0000-0003-000000000008", true, false, 30)
        ];

        builder.Entity<ComponentTypeProperty>().HasData(links.Select((link, index) =>
            new ComponentTypeProperty
            {
                Id = Guid.Parse($"00000000-0000-0000-0004-{index + 1:000000000000}"),
                ComponentTypeId = Guid.Parse(link.ComponentTypeId),
                PropertyId = Guid.Parse(link.PropertyId),
                IsRequired = link.Required,
                IsVariantSpecific = link.Variant,
                SortOrder = link.SortOrder,
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            }));
    }

    private static ComponentType CreateComponentType(string id, string code, string name)
    {
        DateTime timestamp = SeedTimestamp();
        return new ComponentType
        {
            Id = Guid.Parse(id),
            Code = code,
            Name = name,
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        };
    }

    private static void SeedPropertyAliases(ModelBuilder builder)
    {
        (string PropertyId, string Alias)[] aliases =
        [
            ("00000000-0000-0000-0003-000000000001", "kv rating"),
            ("00000000-0000-0000-0003-000000000002", "maximum current"),
            ("00000000-0000-0000-0003-000000000002", "max current"),
            ("00000000-0000-0000-0003-000000000002", "continuous current"),
            ("00000000-0000-0000-0003-000000000003", "input voltage"),
            ("00000000-0000-0000-0003-000000000003", "voltage range"),
            ("00000000-0000-0000-0003-000000000003", "supported voltage"),
            ("00000000-0000-0000-0003-000000000004", "shaft diameter"),
            ("00000000-0000-0000-0003-000000000005", "propeller size"),
            ("00000000-0000-0000-0003-000000000005", "prop size"),
            ("00000000-0000-0000-0003-000000000006", "wheelbase"),
            ("00000000-0000-0000-0003-000000000007", "mount width"),
            ("00000000-0000-0000-0003-000000000008", "mount height"),
            ("00000000-0000-0000-0003-000000000009", "cell count"),
            ("00000000-0000-0000-0003-000000000009", "lipo cells"),
            ("00000000-0000-0000-0003-000000000010", "battery capacity"),
            ("00000000-0000-0000-0003-000000000010", "capacity"),
            ("00000000-0000-0000-0003-000000000011", "connector type"),
            ("00000000-0000-0000-0003-000000000011", "connector")
        ];

        DateTime timestamp = SeedTimestamp();
        builder.Entity<PropertyAlias>().HasData(aliases.Select((alias, index) =>
            new PropertyAlias
            {
                Id = Guid.Parse($"00000000-0000-0000-0006-{index + 1:000000000000}"),
                PropertyId = Guid.Parse(alias.PropertyId),
                Alias = alias.Alias,
                NormalizedAlias = SpecificationAliasNormalizer.Normalize(alias.Alias),
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            }));
    }

    private static Property CreateProperty(
        string id,
        string code,
        string name,
        SpecificationDataType dataType,
        string? unitDefinitionId)
    {
        DateTime timestamp = SeedTimestamp();
        return new Property
        {
            Id = Guid.Parse(id),
            Code = code,
            Name = name,
            DataType = dataType,
            UnitDefinitionId = unitDefinitionId is null ? null : Guid.Parse(unitDefinitionId),
            IsFilterable = true,
            IsCompatibilityRelevant = true,
            AllowsMultipleValues = false,
            CreatedAt = timestamp,
            UpdatedAt = timestamp
        };
    }

    private static DateTime SeedTimestamp()
        => new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}
