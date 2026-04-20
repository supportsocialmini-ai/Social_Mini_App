using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social_Mini_App.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_PG : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isPostgres = migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL";

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    ConversationId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: isPostgres ? "character varying(200)" : "nvarchar(200)", maxLength: 200, nullable: true),
                    IsGroupChat = table.Column<bool>(type: isPostgres ? "boolean" : "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: isPostgres ? "timestamp without time zone" : "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.ConversationId);
                });

            migrationBuilder.CreateTable(
                name: "Friendships",
                columns: table => new
                {
                    FriendshipId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: isPostgres ? "timestamp without time zone" : "datetime2", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: isPostgres ? "timestamp without time zone" : "datetime2", nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: isPostgres ? "timestamp without time zone" : "datetime2", nullable: true),
                    Status = table.Column<string>(type: isPostgres ? "character varying(20)" : "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friendships", x => x.FriendshipId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: isPostgres ? "text" : "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: isPostgres ? "text" : "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: isPostgres ? "text" : "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: isPostgres ? "text" : "nvarchar(max)", nullable: false),
                    AvatarUrl = table.Column<string>(type: isPostgres ? "text" : "nvarchar(max)", nullable: true),
                    Bio = table.Column<string>(type: isPostgres ? "character varying(255)" : "nvarchar(255)", maxLength: 255, nullable: true),
                    IsVerified = table.Column<bool>(type: isPostgres ? "boolean" : "bit", nullable: false),
                    IsActive = table.Column<bool>(type: isPostgres ? "boolean" : "bit", nullable: false),
                    VerificationToken = table.Column<string>(type: isPostgres ? "text" : "nvarchar(max)", nullable: true),
                    PasswordResetToken = table.Column<string>(type: isPostgres ? "text" : "nvarchar(max)", nullable: true),
                    ResetTokenExpires = table.Column<DateTime>(type: isPostgres ? "timestamp without time zone" : "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: isPostgres ? "timestamp without time zone" : "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: isPostgres ? "timestamp without time zone" : "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "ConversationParticipants",
                columns: table => new
                {
                    ParticipantId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: isPostgres ? "timestamp without time zone" : "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationParticipants", x => x.ParticipantId);
                    table.ForeignKey(
                        name: "FK_ConversationParticipants_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "ConversationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConversationParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FriendshipMembers",
                columns: table => new
                {
                    MemberId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    FriendshipId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    IsRequestSender = table.Column<bool>(type: isPostgres ? "boolean" : "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendshipMembers", x => x.MemberId);
                    table.ForeignKey(
                        name: "FK_FriendshipMembers_Friendships_FriendshipId",
                        column: x => x.FriendshipId,
                        principalTable: "Friendships",
                        principalColumn: "FriendshipId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FriendshipMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    SenderId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    MessageContent = table.Column<string>(type: isPostgres ? "character varying(500)" : "nvarchar(500)", maxLength: 500, nullable: false),
                    IsRead = table.Column<bool>(type: isPostgres ? "boolean" : "bit", nullable: false),
                    ImageUrl = table.Column<string>(type: isPostgres ? "text" : "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: isPostgres ? "timestamp without time zone" : "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "ConversationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    ReceiverId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    SenderId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: isPostgres ? "text" : "nvarchar(max)", nullable: false),
                    PostId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: true),
                    NotificationType = table.Column<string>(type: isPostgres ? "text" : "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: isPostgres ? "boolean" : "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: isPostgres ? "timestamp without time zone" : "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    PostId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    PostContent = table.Column<string>(type: isPostgres ? "character varying(1000)" : "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Privacy = table.Column<string>(type: isPostgres ? "character varying(20)" : "nvarchar(20)", maxLength: 20, nullable: false),
                    ImageUrl = table.Column<string>(type: isPostgres ? "text" : "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: isPostgres ? "timestamp without time zone" : "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: isPostgres ? "timestamp without time zone" : "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.PostId);
                    table.ForeignKey(
                        name: "FK_Posts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    CommentId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    CommentContent = table.Column<string>(type: isPostgres ? "character varying(200)" : "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: isPostgres ? "timestamp without time zone" : "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: isPostgres ? "timestamp without time zone" : "datetime2", nullable: true),
                    UserId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    PostId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    ParentCommentId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "Comments",
                        principalColumn: "CommentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    PostId = table.Column<Guid>(type: isPostgres ? "uuid" : "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: isPostgres ? "timestamp without time zone" : "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => new { x.UserId, x.PostId });
                    table.ForeignKey(
                        name: "FK_Likes_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Likes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentId",
                table: "Comments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PostId",
                table: "Comments",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationParticipants_ConversationId",
                table: "ConversationParticipants",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationParticipants_UserId",
                table: "ConversationParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendshipMembers_FriendshipId",
                table: "FriendshipMembers",
                column: "FriendshipId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendshipMembers_UserId",
                table: "FriendshipMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_PostId",
                table: "Likes",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId",
                table: "Messages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SenderId",
                table: "Notifications",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_UserId",
                table: "Posts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "ConversationParticipants");

            migrationBuilder.DropTable(
                name: "FriendshipMembers");

            migrationBuilder.DropTable(
                name: "Likes");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Friendships");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
