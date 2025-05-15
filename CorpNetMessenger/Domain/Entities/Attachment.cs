using System.ComponentModel.DataAnnotations;

namespace CorpNetMessenger.Domain.Entities
{
    public class Attachment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string MessageId { get; set; } = null!;

        [MaxLength(255)]
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public byte[] FileData { get; set; } = null!;
        public long FileLength { get; set; }

        public virtual Message Message { get; set; } = null!;
    }
}
