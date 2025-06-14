﻿namespace CorpNetMessenger.Application.Converters
{
    public static class BytesToStringConverter
    {
        /// <summary>
        /// Конвертирование размера файла из байтов в килобайты, мегабайты, гигабайты и т.д.
        /// </summary>
        /// <param name="fileLength">Количество байтов</param>
        /// <returns>Возвращает размер файла в килобайты, мегабайты, гигабайты и т.д.</returns>
        public static string Convert(long fileLength)
        {
            string[] suf = { "Byt", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (fileLength == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(fileLength);
            int place = System.Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(fileLength) * num).ToString() + suf[place];
        }
    }
}
