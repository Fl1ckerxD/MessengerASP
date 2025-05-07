namespace CorpNetMessenger.Domain.Entities
{
    public class MessageUser
    {
        public int MessageId { get; set; }
        public int UserId { get; set; }
        public bool? Read { get; set; }

        public virtual Message Message { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
