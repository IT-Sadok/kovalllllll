using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DroneBuilder.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ComponentSpecs : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ComponentSpecs",
            columns: table => new
            {
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
                RfConnector = table.Column<int>(type: "integer", nullable: true),
                Cells = table.Column<int>(type: "integer", nullable: true),
                CapacityMah = table.Column<int>(type: "integer", nullable: true),
                CRating = table.Column<int>(type: "integer", nullable: true),
                BatteryConnector = table.Column<int>(type: "integer", nullable: true),
                VideoSystem = table.Column<int>(type: "integer", nullable: true),
                WidthMm = table.Column<int>(type: "integer", nullable: true),
                MountPattern = table.Column<int>(type: "integer", nullable: true),
                MinCells = table.Column<int>(type: "integer", nullable: true),
                MaxCells = table.Column<int>(type: "integer", nullable: true),
                ContinuousCurrentA = table.Column<decimal>(type: "numeric(5,1)", precision: 5, scale: 1, nullable: true),
                MaxPropSizeInch = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                FcMountPatterns = table.Column<int[]>(type: "integer[]", nullable: true),
                MotorMountPatterns = table.Column<int[]>(type: "integer[]", nullable: true),
                CameraWidthMm = table.Column<int>(type: "integer", nullable: true),
                StatorSize = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                Kv = table.Column<int>(type: "integer", nullable: true),
                MaxCurrentA = table.Column<decimal>(type: "numeric(5,1)", precision: 5, scale: 1, nullable: true),
                ShaftMm = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                DiameterInch = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                PitchInch = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                BladeCount = table.Column<int>(type: "integer", nullable: true),
                HubMm = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                Protocol = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ComponentSpecs", x => x.ProductId);
                table.ForeignKey(
                    name: "FK_ComponentSpecs_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ComponentSpecs_Type",
            table: "ComponentSpecs",
            column: "Type");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ComponentSpecs");
    }
}
