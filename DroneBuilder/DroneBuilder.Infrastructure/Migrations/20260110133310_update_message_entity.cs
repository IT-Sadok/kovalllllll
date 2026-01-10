using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DroneBuilder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update_message_entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QueueName",
                table: "Messages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 13, 33, 9, 93, DateTimeKind.Utc).AddTicks(2294));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QueueName",
                table: "Messages");

            migrationBuilder.UpdateData(
                table: "Warehouses",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 1, 6, 19, 25, 7, 630, DateTimeKind.Utc).AddTicks(9418));
        }
    }
}
