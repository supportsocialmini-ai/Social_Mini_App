using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social_Mini_App.Migrations
{
    /// <inheritdoc />
    public partial class AddReportSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "SystemSettings",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModified",
                table: "SystemSettings",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "timestamp");

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportId = table.Column<Guid>(nullable: false),
                    ReporterId = table.Column<Guid>(nullable: false),
                    TargetId = table.Column<Guid>(nullable: false),
                    TargetType = table.Column<string>(maxLength: 20, nullable: false),
                    Reason = table.Column<string>(maxLength: 100, nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: true),
                    Status = table.Column<string>(maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ResolvedAt = table.Column<DateTime>(nullable: true),
                    ResolvedById = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_Reports_Users_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reports_Users_ResolvedById",
                        column: x => x.ResolvedById,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingId",
                keyValue: new Guid("f2a4f4d2-d890-4e7a-9391-0300fc749005"),
                column: "LastModified",
                value: new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReporterId",
                table: "Reports",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ResolvedById",
                table: "Reports",
                column: "ResolvedById");

            // SEED SAMPLE REPORT DATA
            migrationBuilder.InsertData(
                table: "Reports",
                columns: new[] { "ReportId", "ReporterId", "TargetId", "TargetType", "Reason", "Description", "Status", "CreatedAt" },
                values: new object[] { 
                    Guid.NewGuid(), 
                    new Guid("f2a4f4d2-d890-4e7a-9391-0300fc749003"), // Reporter: Admin
                    new Guid("f2a4f4d2-d890-4e7a-9391-0300fc749003"), // Target: Self (for testing)
                    "User", 
                    "Spam", 
                    "This is a sample report to test the Admin Dashboard.", 
                    "Pending", 
                    DateTime.Now 
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "SystemSettings",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<byte[]>(
                name: "LastModified",
                table: "SystemSettings",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingId",
                keyValue: new Guid("f2a4f4d2-d890-4e7a-9391-0300fc749005"),
                column: "LastModified",
                value: new byte[] { 8, 222, 72, 200, 180, 248, 0, 0 });
        }
    }
}
