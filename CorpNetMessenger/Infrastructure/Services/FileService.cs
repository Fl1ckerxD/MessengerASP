using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Services;

namespace CorpNetMessenger.Infrastructure.Services
{
    public class FileService : IFileService
    {
        /// <summary>
        /// Обрабатывает коллекцию загруженных файлов и преобразует их в список вложений
        /// </summary>
        /// <param name="files">Коллекция загруженных файлов (IFormFileCollection)</param>
        /// <returns>Список объектов Attachment с данными файлов</returns>
        /// <exception cref="ArgumentException">При превышении максимального размера файла (10MB)</exception>
        public async Task<List<Attachment>> ProcessFilesAsync(IFormFileCollection files)
        {
            var result = new List<Attachment>();
            if (files == null || files.Count == 0)
                return result;

            foreach (var file in files)
            {
                if (file.Length > 10 * 1024 * 1024) // 10 MB
                    throw new ArgumentException($"Файл {file.FileName} слишком большой");

                using var memoryStream = new MemoryStream();
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
