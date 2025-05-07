using System.Text.RegularExpressions;

namespace CorpNetMessenger.Domain.Entities
{
    public class ChatUser
    {
        public int UserId { get; set; }
        public int ChatId { get; set; }

        public User User { get; set; }
        public Chat Chat { get; set; }
    }
}
