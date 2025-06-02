namespace CorpNetMessenger.Web.Areas.Messaging.ViewModels
{
    public class ChatViewModel
    {
        public ContactPanelViewModel Contacts { get; set; }
        public IEnumerable<MessageViewModel> Chat { get; set; }
    }
}
