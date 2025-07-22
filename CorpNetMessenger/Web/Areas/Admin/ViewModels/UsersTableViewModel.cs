namespace CorpNetMessenger.Web.Areas.Admin.ViewModels
{
    public class UsersTableViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public string PostName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}