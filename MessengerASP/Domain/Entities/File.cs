using System.ComponentModel.DataAnnotations;

namespace CorpNetMessenger.Domain.Entities
{
    public class File
    {
        public int Id { get; set; }
        public int MessageId { get; set; }

        [MaxLength(100)]
        public string FileName { get; set; } = null!;

        [MaxLength(10)]
        public string FileExtension { get; set; } = null!;
        public byte[] FileData { get; set; } = null!;
        public long FileLength { get; set; }

        public virtual Message Message { get; set; } = null!;
    }
}
