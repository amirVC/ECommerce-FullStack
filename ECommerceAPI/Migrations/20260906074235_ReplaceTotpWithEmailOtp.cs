using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAPI.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceTotpWithEmailOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TwoFactorSecret",
                table: "Users",
                newName: "TwoFactorOtpCodeHash");

            migrationBuilder.AddColumn<int>(
                name: "TwoFactorOtpAttempts",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "TwoFactorOtpExpiresAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TwoFactorOtpAttempts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFactorOtpExpiresAt",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "TwoFactorOtpCodeHash",
                table: "Users",
                newName: "TwoFactorSecret");
        }
    }
}
