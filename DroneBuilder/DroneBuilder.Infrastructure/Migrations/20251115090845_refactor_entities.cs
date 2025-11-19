using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DroneBuilder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class refactor_entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductProperty_Products_ProductId",
                table: "ProductProperty");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductProperty_Properties_PropertyId",
                table: "ProductProperty");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyValue_Properties_PropertyId",
                table: "PropertyValue");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyValue_Values_ValueId",
                table: "PropertyValue");

            migrationBuilder.RenameColumn(
                name: "ValueText",
                table: "Values",
                newName: "Text");

            migrationBuilder.RenameColumn(
                name: "ValueId",
                table: "PropertyValue",
                newName: "ValuesId");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "PropertyValue",
                newName: "PropertiesId");

            migrationBuilder.RenameIndex(
                name: "IX_PropertyValue_ValueId",
                table: "PropertyValue",
                newName: "IX_PropertyValue_ValuesId");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "ProductProperty",
                newName: "PropertiesId");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "ProductProperty",
                newName: "ProductsId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductProperty_PropertyId",
                table: "ProductProperty",
                newName: "IX_ProductProperty_PropertiesId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductProperty_Products_ProductsId",
                table: "ProductProperty",
                column: "ProductsId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductProperty_Properties_PropertiesId",
                table: "ProductProperty",
                column: "PropertiesId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyValue_Properties_PropertiesId",
                table: "PropertyValue",
                column: "PropertiesId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyValue_Values_ValuesId",
                table: "PropertyValue",
                column: "ValuesId",
                principalTable: "Values",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductProperty_Products_ProductsId",
                table: "ProductProperty");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductProperty_Properties_PropertiesId",
                table: "ProductProperty");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyValue_Properties_PropertiesId",
                table: "PropertyValue");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyValue_Values_ValuesId",
                table: "PropertyValue");

            migrationBuilder.RenameColumn(
                name: "Text",
                table: "Values",
                newName: "ValueText");

            migrationBuilder.RenameColumn(
                name: "ValuesId",
                table: "PropertyValue",
                newName: "ValueId");

            migrationBuilder.RenameColumn(
                name: "PropertiesId",
                table: "PropertyValue",
                newName: "PropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_PropertyValue_ValuesId",
                table: "PropertyValue",
                newName: "IX_PropertyValue_ValueId");

            migrationBuilder.RenameColumn(
                name: "PropertiesId",
                table: "ProductProperty",
                newName: "PropertyId");

            migrationBuilder.RenameColumn(
                name: "ProductsId",
                table: "ProductProperty",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductProperty_PropertiesId",
                table: "ProductProperty",
                newName: "IX_ProductProperty_PropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductProperty_Products_ProductId",
                table: "ProductProperty",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductProperty_Properties_PropertyId",
                table: "ProductProperty",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyValue_Properties_PropertyId",
                table: "PropertyValue",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyValue_Values_ValueId",
                table: "PropertyValue",
                column: "ValueId",
                principalTable: "Values",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
