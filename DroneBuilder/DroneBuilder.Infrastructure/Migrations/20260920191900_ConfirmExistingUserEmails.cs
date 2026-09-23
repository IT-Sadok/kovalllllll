using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DroneBuilder.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ConfirmExistingUserEmails : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Sign-in now requires a confirmed email. Accounts that existed before this rule
        // were able to sign in, so they are grandfathered in rather than locked out --
        // there is no way for them to request a new confirmation link.
        migrationBuilder.Sql(
            """
            UPDATE "AspNetUsers" SET "EmailConfirmed" = TRUE WHERE "EmailConfirmed" = FALSE;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Which accounts were unconfirmed before the update is not recorded, so this cannot be reversed.
    }
}
