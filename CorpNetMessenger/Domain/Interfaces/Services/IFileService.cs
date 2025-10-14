using CorpNetMessenger.Domain.Entities;

namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IFileService
    {
        Task<List<Attachment>> ProcessFilesAsync(IFormFileCollection files, CancellationToken cancellationToken = default);
    }
}
