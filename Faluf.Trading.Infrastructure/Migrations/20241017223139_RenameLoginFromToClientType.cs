using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Faluf.Trading.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameLoginFromToClientType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RevokedAtUTC",
                table: "RefreshTokens",
                newName: "LockoutEndUTC");

            migrationBuilder.RenameColumn(
                name: "LoginFrom",
                table: "RefreshTokens",
                newName: "ClientType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LockoutEndUTC",
                table: "RefreshTokens",
                newName: "RevokedAtUTC");

            migrationBuilder.RenameColumn(
                name: "ClientType",
                table: "RefreshTokens",
                newName: "LoginFrom");
        }
    }
}
