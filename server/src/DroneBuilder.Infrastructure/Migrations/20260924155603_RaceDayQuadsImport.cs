using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DroneBuilder.Infrastructure.Migrations;

/// <inheritdoc />
public partial class RaceDayQuadsImport : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ExternalId",
            table: "Products",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ExternalSource",
            table: "Products",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "GroupId",
            table: "Products",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "NeedsReview",
            table: "Products",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "SourceUrl",
            table: "Products",
            type: "character varying(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "VariantName",
            table: "Products",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "ImportRuns",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Added = table.Column<int>(type: "integer", nullable: false),
                Updated = table.Column<int>(type: "integer", nullable: false),
                Skipped = table.Column<int>(type: "integer", nullable: false),
                NeedsReview = table.Column<int>(type: "integer", nullable: false),
                Failed = table.Column<int>(type: "integer", nullable: false),
                Error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_ImportRuns", x => x.Id));

        migrationBuilder.CreateTable(
            name: "ProductGroups",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                ExternalSource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                ExternalId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_ProductGroups", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Products_ExternalSource_ExternalId",
            table: "Products",
            columns: new[] { "ExternalSource", "ExternalId" },
            unique: true,
            filter: "\"ExternalId\" IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Products_GroupId",
            table: "Products",
            column: "GroupId");

        migrationBuilder.CreateIndex(
            name: "IX_ImportRuns_CreatedAt",
            table: "ImportRuns",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_ProductGroups_ExternalSource_ExternalId",
            table: "ProductGroups",
            columns: new[] { "ExternalSource", "ExternalId" },
            unique: true,
            filter: "\"ExternalId\" IS NOT NULL");

        migrationBuilder.AddForeignKey(
            name: "FK_Products_ProductGroups_GroupId",
            table: "Products",
            column: "GroupId",
            principalTable: "ProductGroups",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Products_ProductGroups_GroupId",
            table: "Products");

        migrationBuilder.DropTable(
            name: "ImportRuns");

        migrationBuilder.DropTable(
            name: "ProductGroups");

        migrationBuilder.DropIndex(
            name: "IX_Products_ExternalSource_ExternalId",
            table: "Products");

        migrationBuilder.DropIndex(
            name: "IX_Products_GroupId",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "ExternalId",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "ExternalSource",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "GroupId",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "NeedsReview",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "SourceUrl",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "VariantName",
            table: "Products");
    }
}
