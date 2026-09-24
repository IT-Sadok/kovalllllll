using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DroneBuilder.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ProductCategoryEnum : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE "Products" ALTER COLUMN "Category" DROP DEFAULT;
            ALTER TABLE "Products" ALTER COLUMN "Category" TYPE integer USING (
                CASE regexp_replace(lower("Category"), '[^a-z0-9]', '', 'g')
                    WHEN 'frame' THEN 0 WHEN 'frames' THEN 0
                    WHEN 'motor' THEN 1 WHEN 'motors' THEN 1
                    WHEN 'propeller' THEN 2 WHEN 'propellers' THEN 2 WHEN 'prop' THEN 2 WHEN 'props' THEN 2
                    WHEN 'flightcontroller' THEN 3 WHEN 'flightcontrollers' THEN 3 WHEN 'fc' THEN 3
                    WHEN 'esc' THEN 4 WHEN 'escs' THEN 4
                    WHEN 'battery' THEN 5 WHEN 'batteries' THEN 5
                    WHEN 'videotransmitter' THEN 6 WHEN 'videotransmitters' THEN 6 WHEN 'vtx' THEN 6
                    WHEN 'camera' THEN 7 WHEN 'cameras' THEN 7
                    WHEN 'receiver' THEN 8 WHEN 'receivers' THEN 8
                    WHEN 'antenna' THEN 9 WHEN 'antennas' THEN 9
                    WHEN 'stack' THEN 10 WHEN 'stacks' THEN 10 WHEN 'aio' THEN 10
                    WHEN 'goggles' THEN 100
                    WHEN 'radio' THEN 101 WHEN 'radios' THEN 101
                    WHEN 'charger' THEN 102 WHEN 'chargers' THEN 102
                    WHEN 'tool' THEN 103 WHEN 'tools' THEN 103
                    WHEN 'racing' THEN 104 WHEN 'photography' THEN 104 WHEN 'industrial' THEN 104
                    WHEN 'military' THEN 104 WHEN 'consumer' THEN 104 WHEN 'fpv' THEN 104
                    WHEN 'drone' THEN 104 WHEN 'drones' THEN 104 WHEN 'quadcopter' THEN 104
                    ELSE 105
                END);
            """);

        migrationBuilder.Sql("""
            UPDATE "Products" p SET "Category" = s."Type"
            FROM "ComponentSpecs" s
            WHERE s."ProductId" = p."Id";
            """);

        migrationBuilder.CreateIndex(
            name: "IX_Products_Category",
            table: "Products",
            column: "Category");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Products_Category",
            table: "Products");

        migrationBuilder.Sql("""
            ALTER TABLE "Products" ALTER COLUMN "Category" TYPE character varying(100) USING (
                CASE "Category"
                    WHEN 0 THEN 'Frame' WHEN 1 THEN 'Motor' WHEN 2 THEN 'Propeller'
                    WHEN 3 THEN 'FlightController' WHEN 4 THEN 'Esc' WHEN 5 THEN 'Battery'
                    WHEN 6 THEN 'VideoTransmitter' WHEN 7 THEN 'Camera' WHEN 8 THEN 'Receiver'
                    WHEN 9 THEN 'Antenna' WHEN 10 THEN 'Stack' WHEN 100 THEN 'Goggles'
                    WHEN 101 THEN 'Radio' WHEN 102 THEN 'Charger' WHEN 103 THEN 'Tool'
                    WHEN 104 THEN 'ReadyToFly' ELSE 'Accessory'
                END);
            """);
    }
}
