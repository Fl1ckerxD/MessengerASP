namespace CorpNetMessenger.Web.Areas.Messaging.ViewModels
{
    public class MessageViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsMine { get; set; } = false;
        public IEnumerable<AttachmentViewModel> Attachments { get; set; } = Enumerable.Empty<AttachmentViewModel>();
    }
}
