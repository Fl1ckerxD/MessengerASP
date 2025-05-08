using System.ComponentModel.DataAnnotations;

namespace CorpNetMessenger.Domain.Entities
{
    public class Department
    {
        public Department()
        {
            Chats = new HashSet<Chat>();
            Users = new HashSet<User>();
            Posts = new HashSet<DepartmentPost>();
        }

        public int Id { get; set; }

        [MaxLength(150)]
        public string Title { get; set; } = null!;

        public virtual ICollection<Chat> Chats { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<DepartmentPost> Posts { get; set; }
    }
}
