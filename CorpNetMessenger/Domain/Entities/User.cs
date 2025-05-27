using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CorpNetMessenger.Domain.Entities
{
    public class User : IdentityUser
    {
        public User()
        {
            MessageUsers = new HashSet<MessageUser>();
            Messages = new HashSet<Message>();
            Chats = new HashSet<ChatUser>();
            Id = Guid.NewGuid().ToString();
        }

        [MaxLength(30)] 
        public string LastName { get; set; } = null!;

        [MaxLength(20)] 
        public string Name { get; set; } = null!;

        [MaxLength(35)] 
        public string? Patronymic { get; set; }

        public byte[]? Image { get; set; }
        public int? StatusId { get; set; }
        public int? PostId { get; set; }
        public int? DepartmentId { get; set; }

        public virtual Department? Department { get; set; }
        public virtual Post? Post { get; set; }
        public virtual Status? Status { get; set; }

        public virtual ICollection<MessageUser> MessageUsers { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<ChatUser> Chats { get; set; }
    }
}
