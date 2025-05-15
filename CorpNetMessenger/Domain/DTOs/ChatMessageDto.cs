using CorpNetMessenger.Domain.Entities;

namespace CorpNetMessenger.Domain.DTOs
{
    public class ChatMessageDto
    {
        public string Text { get; set; } = string.Empty;
        public string ChatId { get; set; } = string.Empty;
        public List<Attachment> Files { get; set; } = new();
    }
}
