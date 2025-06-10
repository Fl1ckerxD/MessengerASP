using System.Linq;

namespace CorpNetMessenger.Domain.DTOs
{
    public class AttachmentDto
    {
        public string Id { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileSize {  get; set; } = string.Empty;
        public bool IsImage { get; set; } = false;
    }
}
