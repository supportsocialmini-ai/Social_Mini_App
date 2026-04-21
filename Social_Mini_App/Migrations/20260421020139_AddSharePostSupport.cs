using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social_Mini_App.Migrations
{
    /// <inheritdoc />
    public partial class AddSharePostSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isPostgres = migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL";

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalPostId",
                table: "Posts",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_OriginalPostId",
                table: "Posts",
                column: "OriginalPostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Posts_OriginalPostId",
                table: "Posts",
                column: "OriginalPostId",
                principalTable: "Posts",
                principalColumn: "PostId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Posts_OriginalPostId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_OriginalPostId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "OriginalPostId",
                table: "Posts");
        }
    }
}
