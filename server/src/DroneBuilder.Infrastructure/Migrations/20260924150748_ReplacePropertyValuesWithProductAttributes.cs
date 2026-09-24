using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DroneBuilder.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ReplacePropertyValuesWithProductAttributes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Manufacturer",
            table: "Products",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "WeightGrams",
            table: "Products",
            type: "numeric(7,1)",
            precision: 7,
            scale: 1,
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "MaxThrustGrams",
            table: "ComponentSpecs",
            type: "integer",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "ProductAttributes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                SortOrder = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductAttributes", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductAttributes_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Products_Manufacturer",
            table: "Products",
            column: "Manufacturer");

        migrationBuilder.CreateIndex(
            name: "IX_ProductAttributes_ProductId",
            table: "ProductAttributes",
            column: "ProductId");

        migrationBuilder.Sql("""
            INSERT INTO "ProductAttributes" ("Id", "ProductId", "Name", "Value", "SortOrder")
            SELECT gen_random_uuid(),
                   ppv."ProductId",
                   p."Name",
                   LEFT(string_agg(v."Text", ', ' ORDER BY v."Text"), 500),
                   (ROW_NUMBER() OVER (PARTITION BY ppv."ProductId" ORDER BY p."Name") - 1)::int
            FROM "ProductPropertyValues" ppv
            JOIN "Properties" p ON p."Id" = ppv."PropertyId"
            JOIN "Values" v ON v."Id" = ppv."ValueId"
            GROUP BY ppv."ProductId", p."Id", p."Name";
            """);

        migrationBuilder.DropTable(
            name: "ProductPropertyValues");

        migrationBuilder.DropTable(
            name: "PropertyValue");

        migrationBuilder.DropTable(
            name: "Properties");

        migrationBuilder.DropTable(
            name: "Values");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ProductAttributes");

        migrationBuilder.DropIndex(
            name: "IX_Products_Manufacturer",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "Manufacturer",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "WeightGrams",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "MaxThrustGrams",
            table: "ComponentSpecs");

        migrationBuilder.CreateTable(
            name: "Properties",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Properties", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Values",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Text = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Values", x => x.Id));

        migrationBuilder.CreateTable(
            name: "ProductPropertyValues",
            columns: table => new
            {
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                ValueId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductPropertyValues", x => new { x.ProductId, x.PropertyId, x.ValueId });
                table.ForeignKey(
                    name: "FK_ProductPropertyValues_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProductPropertyValues_Properties_PropertyId",
                    column: x => x.PropertyId,
                    principalTable: "Properties",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProductPropertyValues_Values_ValueId",
                    column: x => x.ValueId,
                    principalTable: "Values",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PropertyValue",
            columns: table => new
            {
                PropertiesId = table.Column<Guid>(type: "uuid", nullable: false),
                ValuesId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PropertyValue", x => new { x.PropertiesId, x.ValuesId });
                table.ForeignKey(
                    name: "FK_PropertyValue_Properties_PropertiesId",
                    column: x => x.PropertiesId,
                    principalTable: "Properties",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PropertyValue_Values_ValuesId",
                    column: x => x.ValuesId,
                    principalTable: "Values",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProductPropertyValues_PropertyId",
            table: "ProductPropertyValues",
            column: "PropertyId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductPropertyValues_ValueId",
            table: "ProductPropertyValues",
            column: "ValueId");

        migrationBuilder.CreateIndex(
            name: "IX_PropertyValue_ValuesId",
            table: "PropertyValue",
            column: "ValuesId");
    }
}
