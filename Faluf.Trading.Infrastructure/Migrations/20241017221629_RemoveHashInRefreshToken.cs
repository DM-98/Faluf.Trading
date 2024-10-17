using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Faluf.Trading.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHashInRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HashedToken",
                table: "RefreshTokens",
                newName: "Token");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Token",
                table: "RefreshTokens",
                newName: "HashedToken");
        }
    }
}
