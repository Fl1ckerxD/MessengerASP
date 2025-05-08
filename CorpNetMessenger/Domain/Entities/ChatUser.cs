using System.Text.RegularExpressions;

namespace CorpNetMessenger.Domain.Entities
{
    public class ChatUser
    {
        public string UserId { get; set; } = null!;
        public string ChatId { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsAdmin { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual Chat Chat { get; set; } = null!;
    }
}
