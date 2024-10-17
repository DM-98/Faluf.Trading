using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Faluf.Trading.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LoginFromInRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoginFrom",
                table: "RefreshTokens",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoginFrom",
                table: "RefreshTokens");
        }
    }
}
