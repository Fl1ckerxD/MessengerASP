using System.ComponentModel.DataAnnotations;

namespace CorpNetMessenger.Domain.Entities
{
    public class Status
    {
        public Status()
        {
            Users = new HashSet<User>();
        }

        public int Id { get; set; }

        [MaxLength(15)]
        public string Title { get; set; } = null!;

        public virtual ICollection<User> Users { get; set; }
    }
}
