using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DroneBuilder.Infrastructure.Migrations;

/// <inheritdoc />
public partial class CartItemReservedAt : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Backfilled with NOW() rather than the scaffolded DateTime.MinValue: the age of existing
        // reservations is unknown, and MinValue would expire every cart in the database on the
        // first sweep after deployment. They get a full lifetime instead.
        migrationBuilder.AddColumn<DateTime>(
            name: "ReservedAt",
            table: "CartItems",
            type: "timestamp with time zone",
            nullable: false,
            defaultValueSql: "NOW()");

        // The value always comes from the entity, so the column keeps no default of its own.
        migrationBuilder.Sql("""ALTER TABLE "CartItems" ALTER COLUMN "ReservedAt" DROP DEFAULT;""");

        migrationBuilder.CreateIndex(
            name: "IX_CartItems_ReservedAt",
            table: "CartItems",
            column: "ReservedAt");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_CartItems_ReservedAt",
            table: "CartItems");

        migrationBuilder.DropColumn(
            name: "ReservedAt",
            table: "CartItems");
    }
}
