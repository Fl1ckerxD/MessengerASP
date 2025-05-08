namespace CorpNetMessenger.Domain.Entities
{
    public class DepartmentPost
    {
        public int PostId { get; set; }
        public int DepartmentId { get; set; }

        public virtual Post Post { get; set; } = null!;
        public virtual Department Department { get; set; } = null!;
    }
}
