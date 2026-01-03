using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DroneBuilder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update_status_entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 3, 14, 18, 14, 10, DateTimeKind.Utc).AddTicks(4183));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 28, 11, 50, 29, 850, DateTimeKind.Utc).AddTicks(8348));
        }
    }
}
