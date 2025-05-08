namespace CorpNetMessenger.Domain.Entities
{
    public class MessageUser
    {
        public string MessageId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public DateTime ReadAt { get; set; } = DateTime.UtcNow;

        public virtual Message Message { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
