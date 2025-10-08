using CorpNetMessenger.Application.Configs;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Services;

namespace CorpNetMessenger.Infrastructure.Services
{
    public class FileService : IFileService
    {
        /// <summary>
        /// Обрабатывает коллекцию загруженных файлов и преобразует их в список вложений.
        /// </summary>
        /// <param name="files">Коллекция загруженных файлов (IFormFileCollection)</param>
        /// <returns>Список объектов Attachment с данными файлов</returns>
        /// <exception cref="InvalidDataException">Если файл превышает допустимый размер или имеет недопустимый формат</exception>
        public async Task<List<Attachment>> ProcessFilesAsync(IFormFileCollection files)
        {
            var result = new List<Attachment>();

            if (files == null || files.Count == 0)
                return result;

            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                    continue;

                if (file.Length > MessagingOptions.MaxFileSize)
                    throw new InvalidDataException($"Файл {file.FileName} превышает максимально допустимый размер ({MessagingOptions.MaxFileSize} байт).");

                using var memoryStream = new MemoryStream((int)Math.Min(file.Length, int.MaxValue));
                await file.CopyToAsync(memoryStream);

                result.Add(new Attachment
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    FileData = memoryStream.ToArray(),
                    FileLength = file.Length
                });
            }

            return result;
        }
    }
}
