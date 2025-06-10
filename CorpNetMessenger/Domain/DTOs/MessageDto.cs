namespace CorpNetMessenger.Domain.DTOs
{
    public class MessageDto
    {
        public string Id { get; set; } = null!;
        public string Text { get; set; } = string.Empty;
        public UserDto User { get; set; } = new();
        public string SentAt { get; set; } = null!;
        public List<AttachmentDto> Attachments { get; set; } = new();
    }
}
