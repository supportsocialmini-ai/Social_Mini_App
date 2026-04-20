using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Social_Mini_App.Migrations
{
    /// <inheritdoc />
    public partial class SeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Name" },
                values: new object[,]
                {
                    { new Guid("f2a4f4d2-d890-4e7a-9391-0300fc749001"), "Admin" },
                    { new Guid("f2a4f4d2-d890-4e7a-9391-0300fc749002"), "User" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "Email", "FullName", "IsActive", "IsVerified", "PasswordHash", "Username" },
                values: new object[] { new Guid("f2a4f4d2-d890-4e7a-9391-0300fc749003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@socialmini.com", "Quản trị viên", true, true, "$2a$11$aRtwrGOmOjfzLc5JmHat/OURRrSltBiM5XWAHLiga4BZefXkKzVnG", "admin" });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId", "AssignedAt" },
                values: new object[] { new Guid("f2a4f4d2-d890-4e7a-9391-0300fc749001"), new Guid("f2a4f4d2-d890-4e7a-9391-0300fc749003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("f2a4f4d2-d890-4e7a-9391-0300fc749001"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("f2a4f4d2-d890-4e7a-9391-0300fc749002"));
        }
    }
}
