using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailOtps_UserId_Type_IsUsed",
                table: "EmailOtps");

            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "EmailOtps");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "EmailOtps",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "UsedAt",
                table: "EmailOtps",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OtpVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VerificationTokenHash = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtpVerifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailOtps_UserId_Type",
                table: "EmailOtps",
                columns: new[] { "UserId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailOtps_UserId_Type_UsedAt",
                table: "EmailOtps",
                columns: new[] { "UserId", "Type", "UsedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OtpVerifications_UserId_Type_UsedAt",
                table: "OtpVerifications",
                columns: new[] { "UserId", "Type", "UsedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OtpVerifications_VerificationTokenHash",
                table: "OtpVerifications",
                column: "VerificationTokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OtpVerifications");

            migrationBuilder.DropIndex(
                name: "IX_EmailOtps_UserId_Type",
                table: "EmailOtps");

            migrationBuilder.DropIndex(
                name: "IX_EmailOtps_UserId_Type_UsedAt",
                table: "EmailOtps");

            migrationBuilder.DropColumn(
                name: "UsedAt",
                table: "EmailOtps");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "EmailOtps",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "EmailOtps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_EmailOtps_UserId_Type_IsUsed",
                table: "EmailOtps",
                columns: new[] { "UserId", "Type", "IsUsed" });
        }
    }
}
