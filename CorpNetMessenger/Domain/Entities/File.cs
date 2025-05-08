using System.ComponentModel.DataAnnotations;

namespace CorpNetMessenger.Domain.Entities
{
    public class File
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string MessageId { get; set; } = null!;

        [MaxLength(100)]
        public string FileName { get; set; } = null!;

        [MaxLength(10)]
        public string FileExtension { get; set; } = null!;
        public byte[] FileData { get; set; } = null!;
        public long FileLength { get; set; }

        public virtual Message Message { get; set; } = null!;
    }
}
