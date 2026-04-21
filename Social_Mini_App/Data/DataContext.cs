using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Models;
using Social_Mini_App.Models;

namespace MiniSocialNetwork.Data
{
    public class DataContext : DbContext
    {
     
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<FriendshipMember> FriendshipMembers { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Reply> Replies { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Composite Key for Like
            modelBuilder.Entity<Like>()
                .HasKey(l => new { l.UserId, l.PostId });

            // Configure ConversationParticipant many-to-many
            base.OnModelCreating(modelBuilder);

            // Messaging Config
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Group chat Creator
            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.Creator)
                .WithMany()
                .HasForeignKey(c => c.CreatorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ConversationParticipant>()
                .HasOne(cp => cp.User)
                .WithMany()
                .HasForeignKey(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Likes Config

            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Comment Config
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reply Config
            modelBuilder.Entity<Reply>()
                .HasOne(r => r.Comment)
                .WithMany(c => c.Replies)
                .HasForeignKey(r => r.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reply>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Friendship Config
            modelBuilder.Entity<FriendshipMember>()
                .HasOne(fm => fm.User)
                .WithMany()
                .HasForeignKey(fm => fm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FriendshipMember>()
                .HasOne(fm => fm.Friendship)
                .WithMany(f => f.Members)
                .HasForeignKey(fm => fm.FriendshipId)
                .OnDelete(DeleteBehavior.Cascade);

            // RBAC Many-to-Many Configuration
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // Data Seeding cho Roles
            var adminRoleId = Guid.Parse("f2a4f4d2-d890-4e7a-9391-0300fc749001");
            var userRoleId = Guid.Parse("f2a4f4d2-d890-4e7a-9391-0300fc749002");

            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = adminRoleId, Name = "Admin" },
                new Role { RoleId = userRoleId, Name = "User" }
            );

            // Data Seeding cho tài khoản Admin mặc định (admin/admin123)
            var adminUserId = Guid.Parse("f2a4f4d2-d890-4e7a-9391-0300fc749003");
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = adminUserId,
                    Username = "admin",
                    PasswordHash = "$2a$11$aRtwrGOmOjfzLc5JmHat/OURRrSltBiM5XWAHLiga4BZefXkKzVnG", 
                    FullName = "Quản trị viên",
                    Email = "admin@socialmini.com",
                    IsActive = true,
                    IsVerified = true,
                    CreatedAt = new DateTime(2026, 1, 1)
                }
            );

            // Gán Role Admin cho User Admin
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole 
                { 
                    UserId = adminUserId, 
                    RoleId = adminRoleId,
                    AssignedAt = new DateTime(2026, 1, 1) 
                }
            );

            // Post Share Config
            modelBuilder.Entity<Post>()
                .HasOne(p => p.OriginalPost)
                .WithMany()
                .HasForeignKey(p => p.OriginalPostId)
                .OnDelete(DeleteBehavior.SetNull);

            // Seed SystemSettings cho Maintenance Mode
            modelBuilder.Entity<SystemSetting>().HasData(
                new SystemSetting
                {
                    SettingId = Guid.Parse("f2a4f4d2-d890-4e7a-9391-0300fc749005"),
                    Key = "MaintenanceMode",
                    Value = "false",
                    Description = "Global maintenance mode toggle. If true, non-admins are blocked.",
                    LastModified = new DateTime(2026, 1, 1)
                }
            );
        }
    }
}


