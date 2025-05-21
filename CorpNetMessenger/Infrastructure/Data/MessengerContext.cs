using CorpNetMessenger.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CorpNetMessenger.Infrastructure.Data
{
    public class MessengerContext : IdentityDbContext<User, IdentityRole<string>, string>
    {
        public virtual DbSet<Chat> Chats { get; set; } = null!;
        public virtual DbSet<Department> Departments { get; set; } = null!;
        public virtual DbSet<Attachment> Attachments { get; set; } = null!;
        public virtual DbSet<Message> Messages { get; set; } = null!;
        public virtual DbSet<MessageUser> MessageUsers { get; set; } = null!;
        public virtual DbSet<Post> Posts { get; set; } = null!;
        public virtual DbSet<Status> Statuses { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<ChatUser> ChatUsers { get; set; } = null!;

        public MessengerContext(DbContextOptions<MessengerContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Department)
                .WithMany(c => c.Chats)
                .HasForeignKey(c => c.DepartmentId);

            modelBuilder.Entity<ChatUser>()
                .HasKey(cu => new { cu.UserId, cu.ChatId });

            modelBuilder.Entity<ChatUser>()
                .HasOne(cu => cu.User)
                .WithMany(cu => cu.Chats)
                .HasForeignKey(cu => cu.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<ChatUser>()
                .HasOne(cu => cu.Chat)
                .WithMany(cu => cu.Users)
                .HasForeignKey(cu => cu.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<Attachment>()
                .HasOne(f => f.Message)
                .WithMany(f => f.Attachments)
                .HasForeignKey(f => f.MessageId);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Chat)
                .WithMany(m => m.Messages)
                .HasForeignKey(m => m.ChatId);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.User)
                .WithMany(m => m.Messages)
                .HasForeignKey(m => m.UserId);

            modelBuilder.Entity<MessageUser>()
                .HasKey(ms => new { ms.MessageId, ms.UserId });

            modelBuilder.Entity<MessageUser>()
                .HasOne(mu => mu.Message)
                .WithMany(mu => mu.ReadByUsers)
                .HasForeignKey(mu => mu.MessageId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<MessageUser>()
                .HasOne(mu => mu.User)
                .WithMany()
                .HasForeignKey(mu => mu.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<DepartmentPost>()
                .HasKey(dp => new { dp.DepartmentId, dp.PostId });

            modelBuilder.Entity<DepartmentPost>()
                .HasOne(dp => dp.Department)
                .WithMany(dp => dp.Posts)
                .HasForeignKey(dp => dp.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<DepartmentPost>()
                .HasOne(dp => dp.Post)
                .WithMany(dp => dp.Departments)
                .HasForeignKey(dp => dp.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(u => u.Users)
                .HasForeignKey(u => u.DepartmentId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Post)
                .WithMany(u => u.Users)
                .HasForeignKey(u => u.PostId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Status)
                .WithMany(u => u.Users)
                .HasForeignKey(u => u.StatusId);
        }
    }
}
