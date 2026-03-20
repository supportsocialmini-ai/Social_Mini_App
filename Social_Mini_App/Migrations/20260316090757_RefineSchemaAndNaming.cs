using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social_Mini_App.Migrations
{
    /// <inheritdoc />
    public partial class RefineSchemaAndNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Posts",
                newName: "PostContent");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Notifications",
                newName: "NotificationType");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Messages",
                newName: "MessageContent");

            migrationBuilder.RenameColumn(
                name: "IsRequester",
                table: "FriendshipMembers",
                newName: "IsRequestSender");

            migrationBuilder.RenameColumn(
                name: "IsGroup",
                table: "Conversations",
                newName: "IsGroupChat");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Comments",
                newName: "CommentContent");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Posts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "Friendships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedAt",
                table: "Friendships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Comments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "Friendships");

            migrationBuilder.DropColumn(
                name: "RequestedAt",
                table: "Friendships");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "PostContent",
                table: "Posts",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "NotificationType",
                table: "Notifications",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "MessageContent",
                table: "Messages",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "IsRequestSender",
                table: "FriendshipMembers",
                newName: "IsRequester");

            migrationBuilder.RenameColumn(
                name: "IsGroupChat",
                table: "Conversations",
                newName: "IsGroup");

            migrationBuilder.RenameColumn(
                name: "CommentContent",
                table: "Comments",
                newName: "Content");
        }
    }
}
