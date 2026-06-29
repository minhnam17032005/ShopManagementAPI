using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOtpAndEmailOtpSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailOtps_Users_UserId",
                table: "EmailOtps");

            migrationBuilder.DropForeignKey(
                name: "FK_OtpVerifications_Users_UserId",
                table: "OtpVerifications");

            migrationBuilder.DropIndex(
                name: "IX_OtpVerifications_VerificationTokenHash",
                table: "OtpVerifications");

            migrationBuilder.DropIndex(
                name: "IX_EmailOtps_UserId_Type",
                table: "EmailOtps");

            migrationBuilder.DropColumn(
                name: "VerificationTokenHash",
                table: "OtpVerifications");

            migrationBuilder.DropColumn(
                name: "OtpHash",
                table: "EmailOtps");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "OtpVerifications",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                table: "OtpVerifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                table: "OtpVerifications",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "EmailOtps",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                table: "EmailOtps",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                table: "EmailOtps",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_OtpVerifications_Email_Type_UsedAt",
                table: "OtpVerifications",
                columns: new[] { "Email", "Type", "UsedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OtpVerifications_TokenHash",
                table: "OtpVerifications",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailOtps_Email_Type_UsedAt",
                table: "EmailOtps",
                columns: new[] { "Email", "Type", "UsedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailOtps_TokenHash",
                table: "EmailOtps",
                column: "TokenHash",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailOtps_Users_UserId",
                table: "EmailOtps",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OtpVerifications_Users_UserId",
                table: "OtpVerifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailOtps_Users_UserId",
                table: "EmailOtps");

            migrationBuilder.DropForeignKey(
                name: "FK_OtpVerifications_Users_UserId",
                table: "OtpVerifications");

            migrationBuilder.DropIndex(
                name: "IX_OtpVerifications_Email_Type_UsedAt",
                table: "OtpVerifications");

            migrationBuilder.DropIndex(
                name: "IX_OtpVerifications_TokenHash",
                table: "OtpVerifications");

            migrationBuilder.DropIndex(
                name: "IX_EmailOtps_Email_Type_UsedAt",
                table: "EmailOtps");

            migrationBuilder.DropIndex(
                name: "IX_EmailOtps_TokenHash",
                table: "EmailOtps");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "OtpVerifications");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "OtpVerifications");

            migrationBuilder.DropColumn(
                name: "TokenHash",
                table: "OtpVerifications");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "EmailOtps");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "EmailOtps");

            migrationBuilder.DropColumn(
                name: "TokenHash",
                table: "EmailOtps");

            migrationBuilder.AddColumn<string>(
                name: "VerificationTokenHash",
                table: "OtpVerifications",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OtpHash",
                table: "EmailOtps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_OtpVerifications_VerificationTokenHash",
                table: "OtpVerifications",
                column: "VerificationTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailOtps_UserId_Type",
                table: "EmailOtps",
                columns: new[] { "UserId", "Type" });

            migrationBuilder.AddForeignKey(
                name: "FK_EmailOtps_Users_UserId",
                table: "EmailOtps",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OtpVerifications_Users_UserId",
                table: "OtpVerifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
