using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAPI.Migrations
{
    /// <inheritdoc />
    public partial class EmailOtpLoginAndTotpSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TwoFactorOtpExpiresAt",
                table: "Users",
                newName: "LoginOtpExpiresAt");

            migrationBuilder.RenameColumn(
                name: "TwoFactorOtpCodeHash",
                table: "Users",
                newName: "TwoFactorSecret");

            migrationBuilder.RenameColumn(
                name: "TwoFactorOtpAttempts",
                table: "Users",
                newName: "LoginOtpAttempts");

            migrationBuilder.RenameColumn(
                name: "TwoFactorChallengeTokenHash",
                table: "Users",
                newName: "LoginOtpCodeHash");

            migrationBuilder.RenameColumn(
                name: "TwoFactorChallengeTokenExpiresAt",
                table: "Users",
                newName: "LoginChallengeTokenExpiresAt");

            migrationBuilder.AddColumn<bool>(
                name: "LoginChallengeEmailVerified",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LoginChallengeTokenHash",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoginChallengeEmailVerified",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LoginChallengeTokenHash",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "TwoFactorSecret",
                table: "Users",
                newName: "TwoFactorOtpCodeHash");

            migrationBuilder.RenameColumn(
                name: "LoginOtpExpiresAt",
                table: "Users",
                newName: "TwoFactorOtpExpiresAt");

            migrationBuilder.RenameColumn(
                name: "LoginOtpCodeHash",
                table: "Users",
                newName: "TwoFactorChallengeTokenHash");

            migrationBuilder.RenameColumn(
                name: "LoginOtpAttempts",
                table: "Users",
                newName: "TwoFactorOtpAttempts");

            migrationBuilder.RenameColumn(
                name: "LoginChallengeTokenExpiresAt",
                table: "Users",
                newName: "TwoFactorChallengeTokenExpiresAt");
        }
    }
}
