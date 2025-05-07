using System.ComponentModel.DataAnnotations;

namespace CorpNetMessenger.Domain.Entities
{
    public class Chat
    {
        public Chat()
        {
            Messages = new HashSet<Message>();
            Users = new HashSet<ChatUser>();
        }

        public int Id { get; set; }

        [MaxLength(50)]
        public string? Theme { get; set; }
        public int? DepartmentId { get; set; }

        public virtual Department? Department { get; set; }
        public virtual ICollection<Message> Messages { get; set; }

        public virtual ICollection<ChatUser> Users { get; set; }
    }
}
