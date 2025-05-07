using System.ComponentModel.DataAnnotations;

namespace CorpNetMessenger.Domain.Entities
{
    public class Message
    {
        public Message()
        {
            Files = new HashSet<File>();
            MessageUsers = new HashSet<MessageUser>();
        }

        public int Id { get; set; }
        public int ChatId { get; set; }
        public int UserId { get; set; }

        [MaxLength(200)]
        public string Content { get; set; } = null!;
        public DateTime Time { get; set; }

        public virtual Chat Chat { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<File> Files { get; set; }
        public virtual ICollection<MessageUser> MessageUsers { get; set; }
    }
}
