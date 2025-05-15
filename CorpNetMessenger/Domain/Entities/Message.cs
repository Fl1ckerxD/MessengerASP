using System.ComponentModel.DataAnnotations;

namespace CorpNetMessenger.Domain.Entities
{
    public class Message
    {
        public Message()
        {
            Attachments = new HashSet<Attachment>();
            ReadByUsers = new HashSet<MessageUser>();
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();

        [MaxLength(200)]
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = null!;
        public string ChatId { get; set; } = null!;

        public virtual User User { get; set; } = null!;
        public virtual Chat Chat { get; set; } = null!;
        public virtual ICollection<Attachment> Attachments { get; set; }
        public virtual ICollection<MessageUser> ReadByUsers { get; set; }
    }
}
