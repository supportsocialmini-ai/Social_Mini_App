using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social_Mini_App.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupChatFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isPostgres = migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL";

            migrationBuilder.AlterColumn<string>(
                name: "VerificationToken",
                table: "Users",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: isPostgres ? "text" : "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: isPostgres ? "text" : "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ResetTokenExpires",
                table: "Users",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: isPostgres ? "text" : "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: isPostgres ? "text" : "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsVerified",
                table: "Users",
                type: isPostgres ? "boolean" : "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: isPostgres ? "boolean" : "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: isPostgres ? "boolean" : "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: isPostgres ? "boolean" : "bit");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Users",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: isPostgres ? "text" : "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: isPostgres ? "text" : "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "Users",
                type: isPostgres ? "character varying(255)" : "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: isPostgres ? "text" : "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Users",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Posts",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Posts",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Privacy",
                table: "Posts",
                type: isPostgres ? "character varying(20)" : "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "PostContent",
                table: "Posts",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: isPostgres ? "character varying(1000)" : "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Posts",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: isPostgres ? "text" : "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Posts",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "PostId",
                table: "Posts",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "SenderId",
                table: "Notifications",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReceiverId",
                table: "Notifications",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "PostId",
                table: "Notifications",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NotificationType",
                table: "Notifications",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: isPostgres ? "text" : "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: isPostgres ? "text" : "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRead",
                table: "Notifications",
                type: isPostgres ? "boolean" : "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: isPostgres ? "boolean" : "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Notifications",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "NotificationId",
                table: "Notifications",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "SenderId",
                table: "Messages",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "MessageContent",
                table: "Messages",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: isPostgres ? "character varying(500)" : "nvarchar(2000)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRead",
                table: "Messages",
                type: isPostgres ? "boolean" : "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: isPostgres ? "boolean" : "bit");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Messages",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: isPostgres ? "text" : "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Messages",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConversationId",
                table: "Messages",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "MessageId",
                table: "Messages",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Likes",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "PostId",
                table: "Likes",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Likes",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Friendships",
                type: isPostgres ? "character varying(20)" : "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestedAt",
                table: "Friendships",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Friendships",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AcceptedAt",
                table: "Friendships",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "FriendshipId",
                table: "Friendships",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "FriendshipMembers",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequestSender",
                table: "FriendshipMembers",
                type: isPostgres ? "boolean" : "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: isPostgres ? "boolean" : "bit");

            migrationBuilder.AlterColumn<Guid>(
                name: "FriendshipId",
                table: "FriendshipMembers",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberId",
                table: "FriendshipMembers",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Conversations",
                type: isPostgres ? "character varying(200)" : "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsGroupChat",
                table: "Conversations",
                type: isPostgres ? "boolean" : "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: isPostgres ? "boolean" : "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Conversations",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConversationId",
                table: "Conversations",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Conversations",
                type: isPostgres ? "character varying(500)" : "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Conversations",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "ConversationParticipants",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedAt",
                table: "ConversationParticipants",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConversationId",
                table: "ConversationParticipants",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParticipantId",
                table: "ConversationParticipants",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "ConversationParticipants",
                type: isPostgres ? "boolean" : "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Comments",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Comments",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PostId",
                table: "Comments",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentCommentId",
                table: "Comments",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Comments",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "CommentContent",
                table: "Comments",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: isPostgres ? "character varying(200)" : "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<Guid>(
                name: "CommentId",
                table: "Comments",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CreatorId",
                table: "Conversations",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Users_CreatorId",
                table: "Conversations",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var isPostgres = migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL";

            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Users_CreatorId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_CreatorId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "ConversationParticipants");

            migrationBuilder.AlterColumn<string>(
                name: "VerificationToken",
                table: "Users",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: isPostgres ? "text" : "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ResetTokenExpires",
                table: "Users",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: isPostgres ? "text" : "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsVerified",
                table: "Users",
                type: isPostgres ? "boolean" : "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: isPostgres ? "boolean" : "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Users",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: isPostgres ? "text" : "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Users",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Posts",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Posts",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Privacy",
                table: "Posts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "PostContent",
                table: "Posts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Posts",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: isPostgres ? "text" : "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Posts",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "PostId",
                table: "Posts",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "SenderId",
                table: "Notifications",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReceiverId",
                table: "Notifications",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "PostId",
                table: "Notifications",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NotificationType",
                table: "Notifications",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRead",
                table: "Notifications",
                type: isPostgres ? "boolean" : "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Notifications",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "NotificationId",
                table: "Notifications",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "SenderId",
                table: "Messages",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "MessageContent",
                table: "Messages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRead",
                table: "Messages",
                type: isPostgres ? "boolean" : "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Messages",
                type: isPostgres ? "text" : "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: isPostgres ? "text" : "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Messages",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConversationId",
                table: "Messages",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "MessageId",
                table: "Messages",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Likes",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "PostId",
                table: "Likes",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Likes",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Friendships",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequestedAt",
                table: "Friendships",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Friendships",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AcceptedAt",
                table: "Friendships",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "FriendshipId",
                table: "Friendships",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "FriendshipMembers",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequestSender",
                table: "FriendshipMembers",
                type: isPostgres ? "boolean" : "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<Guid>(
                name: "FriendshipId",
                table: "FriendshipMembers",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "MemberId",
                table: "FriendshipMembers",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Conversations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsGroupChat",
                table: "Conversations",
                type: isPostgres ? "boolean" : "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Conversations",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConversationId",
                table: "Conversations",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "ConversationParticipants",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedAt",
                table: "ConversationParticipants",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConversationId",
                table: "ConversationParticipants",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParticipantId",
                table: "ConversationParticipants",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Comments",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Comments",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: isPostgres ? "timestamp without time zone" : "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PostId",
                table: "Comments",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentCommentId",
                table: "Comments",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: isPostgres ? "uuid" : "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Comments",
                type: isPostgres ? "timestamp without time zone" : "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "CommentContent",
                table: "Comments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<Guid>(
                name: "CommentId",
                table: "Comments",
                type: isPostgres ? "uuid" : "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
