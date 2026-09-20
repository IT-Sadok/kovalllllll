using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DroneBuilder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WarehouseItemConcurrencyToken : Migration
    {
        // Deliberately empty. The model maps WarehouseItem's concurrency token to xmin, which is a
        // Postgres system column that already exists on every table -- the scaffolded AddColumn was
        // removed because "column name \"xmin\" conflicts with a system column name". Only the model
        // snapshot needs to record it, so later migrations stay clean.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
