using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DroneBuilder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorWarehouseEntityDateTimeData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 13, 33, 9, 93, DateTimeKind.Utc).AddTicks(2294));
        }
    }
}
