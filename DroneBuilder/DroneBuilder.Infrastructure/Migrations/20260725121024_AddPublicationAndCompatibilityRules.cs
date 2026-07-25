using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DroneBuilder.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddPublicationAndCompatibilityRules : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Products_ProductCategoryId_IsActive",
            table: "Products");

        migrationBuilder.AddColumn<int>(
            name: "PublicationStatus",
            table: "Products",
            type: "integer",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.CreateTable(
            name: "CompatibilityRules",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                LeftComponentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                LeftPropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                RightComponentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                RightPropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                Operator = table.Column<int>(type: "integer", nullable: false),
                FailureMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CompatibilityRules", x => x.Id);
                table.ForeignKey(
                    name: "FK_CompatibilityRules_ComponentTypes_LeftComponentTypeId",
                    column: x => x.LeftComponentTypeId,
                    principalTable: "ComponentTypes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CompatibilityRules_ComponentTypes_RightComponentTypeId",
                    column: x => x.RightComponentTypeId,
                    principalTable: "ComponentTypes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CompatibilityRules_Properties_LeftPropertyId",
                    column: x => x.LeftPropertyId,
                    principalTable: "Properties",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CompatibilityRules_Properties_RightPropertyId",
                    column: x => x.RightPropertyId,
                    principalTable: "Properties",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.InsertData(
            table: "CompatibilityRules",
            columns: new[] { "Id", "Code", "CreatedAt", "FailureMessage", "IsActive", "LeftComponentTypeId", "LeftPropertyId", "Name", "Operator", "RightComponentTypeId", "RightPropertyId", "UpdatedAt" },
            values: new object[,]
            {
                { new Guid("00000000-0000-0000-0007-000000000001"), "motor-current-within-esc", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Motor maximum current exceeds ESC capacity.", true, new Guid("00000000-0000-0000-0002-000000000001"), new Guid("00000000-0000-0000-0003-000000000002"), "Motor current within ESC capacity", 1, new Guid("00000000-0000-0000-0002-000000000004"), new Guid("00000000-0000-0000-0003-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0007-000000000002"), "motor-esc-voltage-overlap", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Motor and ESC voltage ranges do not overlap.", true, new Guid("00000000-0000-0000-0002-000000000001"), new Guid("00000000-0000-0000-0003-000000000003"), "Motor and ESC voltage ranges overlap", 5, new Guid("00000000-0000-0000-0002-000000000004"), new Guid("00000000-0000-0000-0003-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0007-000000000003"), "battery-current-supports-motor", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Battery maximum current is below motor demand.", true, new Guid("00000000-0000-0000-0002-000000000003"), new Guid("00000000-0000-0000-0003-000000000002"), "Battery current supports motor", 2, new Guid("00000000-0000-0000-0002-000000000001"), new Guid("00000000-0000-0000-0003-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0007-000000000004"), "frame-fc-mount-width", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Frame and flight controller mount widths differ.", true, new Guid("00000000-0000-0000-0002-000000000002"), new Guid("00000000-0000-0000-0003-000000000007"), "Frame and flight controller mount width", 0, new Guid("00000000-0000-0000-0002-000000000005"), new Guid("00000000-0000-0000-0003-000000000007"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0007-000000000005"), "frame-fc-mount-height", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Frame and flight controller mount heights differ.", true, new Guid("00000000-0000-0000-0002-000000000002"), new Guid("00000000-0000-0000-0003-000000000008"), "Frame and flight controller mount height", 0, new Guid("00000000-0000-0000-0002-000000000005"), new Guid("00000000-0000-0000-0003-000000000008"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0007-000000000006"), "frame-esc-mount-width", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Frame and ESC mount widths differ.", true, new Guid("00000000-0000-0000-0002-000000000002"), new Guid("00000000-0000-0000-0003-000000000007"), "Frame and ESC mount width", 0, new Guid("00000000-0000-0000-0002-000000000004"), new Guid("00000000-0000-0000-0003-000000000007"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                { new Guid("00000000-0000-0000-0007-000000000007"), "frame-esc-mount-height", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Frame and ESC mount heights differ.", true, new Guid("00000000-0000-0000-0002-000000000002"), new Guid("00000000-0000-0000-0003-000000000008"), "Frame and ESC mount height", 0, new Guid("00000000-0000-0000-0002-000000000004"), new Guid("00000000-0000-0000-0003-000000000008"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
            });

        migrationBuilder.CreateIndex(
            name: "IX_Products_ProductCategoryId_IsActive_PublicationStatus",
            table: "Products",
            columns: new[] { "ProductCategoryId", "IsActive", "PublicationStatus" });

        migrationBuilder.CreateIndex(
            name: "IX_CompatibilityRules_Code",
            table: "CompatibilityRules",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_CompatibilityRules_LeftComponentTypeId_RightComponentTypeId~",
            table: "CompatibilityRules",
            columns: new[] { "LeftComponentTypeId", "RightComponentTypeId", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_CompatibilityRules_LeftPropertyId",
            table: "CompatibilityRules",
            column: "LeftPropertyId");

        migrationBuilder.CreateIndex(
            name: "IX_CompatibilityRules_RightComponentTypeId",
            table: "CompatibilityRules",
            column: "RightComponentTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_CompatibilityRules_RightPropertyId",
            table: "CompatibilityRules",
            column: "RightPropertyId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CompatibilityRules");

        migrationBuilder.DropIndex(
            name: "IX_Products_ProductCategoryId_IsActive_PublicationStatus",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "PublicationStatus",
            table: "Products");

        migrationBuilder.CreateIndex(
            name: "IX_Products_ProductCategoryId_IsActive",
            table: "Products",
            columns: new[] { "ProductCategoryId", "IsActive" });
    }
}
