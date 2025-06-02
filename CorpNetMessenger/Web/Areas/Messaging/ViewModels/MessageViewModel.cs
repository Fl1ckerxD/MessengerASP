namespace CorpNetMessenger.Web.Areas.Messaging.ViewModels
{
    public class MessageViewModel
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public string UserName { get; set; }
        public IEnumerable<AttachmentViewModel> Attachments { get; set; } = Enumerable.Empty<AttachmentViewModel>();
    }
}
