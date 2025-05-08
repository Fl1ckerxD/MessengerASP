using System.ComponentModel.DataAnnotations;

namespace CorpNetMessenger.Domain.Entities
{
    public class Post
    {
        public Post()
        {
            Users = new HashSet<User>();
            Departments = new HashSet<DepartmentPost>();
        }

        public int Id { get; set; }

        [MaxLength(50)]
        public string Title { get; set; } = null!;

        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<DepartmentPost> Departments { get; set; }
    }
}
