namespace CorpNetMessenger.Domain.Entities
{
    public class DepartmentPost
    {
        public int PostId { get; set; }
        public int DepartmentId { get; set; }

        public Post Post { get; set; }
        public Department Department { get; set; }
    }
}
