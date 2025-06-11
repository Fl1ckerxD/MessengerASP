using CorpNetMessenger.Domain.DTOs;

namespace CorpNetMessenger.Web.Areas.Messaging.ViewModels
{
    public class ChatViewModel
    {
        public ContactPanelViewModel Contacts { get; set; }
        public IEnumerable<MessageDto> Chat { get; set; }
    }
}
