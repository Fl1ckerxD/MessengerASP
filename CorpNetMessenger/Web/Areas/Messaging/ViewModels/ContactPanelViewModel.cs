namespace CorpNetMessenger.Web.Areas.Messaging.ViewModels
{
    public class ContactPanelViewModel
    {
        public ContactViewModel CurrentUser { get; set; }
        public IEnumerable<ContactViewModel> Contacts { get; set; }
    }
}
