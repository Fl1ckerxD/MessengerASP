namespace CorpNetMessenger.Application.Common
{
    public static class FileHelper
    {
        private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"];

        public static bool IsImage(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            return ImageExtensions.Contains(ext);
        }
    }
}
