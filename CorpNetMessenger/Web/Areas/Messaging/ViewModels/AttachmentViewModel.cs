namespace CorpNetMessenger.Web.Areas.Messaging.ViewModels
{
    public class AttachmentViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FileSize { get; set; }

        public bool IsImage()
        {
            string[] allowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"];
            var ext = Path.GetExtension(Name).ToLower();
            return allowedExtensions.Contains(ext);
        }
    }
}
