using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DroneBuilder.Infrastructure.Migrations;

/// <inheritdoc />
public partial class SavedBuilds : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Builds",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Builds", x => x.Id);
                table.ForeignKey(
                    name: "FK_Builds_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "BuildItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                BuildId = table.Column<Guid>(type: "uuid", nullable: false),
                ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                Quantity = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BuildItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_BuildItems_Builds_BuildId",
                    column: x => x.BuildId,
                    principalTable: "Builds",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_BuildItems_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_BuildItems_BuildId_ProductId",
            table: "BuildItems",
            columns: new[] { "BuildId", "ProductId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_BuildItems_ProductId",
            table: "BuildItems",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_Builds_UserId_UpdatedAt",
            table: "Builds",
            columns: new[] { "UserId", "UpdatedAt" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "BuildItems");

        migrationBuilder.DropTable(
            name: "Builds");
    }
}
