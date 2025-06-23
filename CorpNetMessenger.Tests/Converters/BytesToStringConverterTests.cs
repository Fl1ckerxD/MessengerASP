using CorpNetMessenger.Application.Converters;

namespace CorpNetMessenger.Tests.Converters
{
    public class BytesToStringConverterTests
    {
        [Theory]
        [InlineData(0, "0Byt")]
        [InlineData(1024, "1KB")]
        [InlineData(1024 * 1024, "1MB")]
        [InlineData(1.51 * 1024 * 1024, "1.5MB")]
        [InlineData(1.75 * 1024 * 1024, "1.8MB")]
        public void FormatBytes_Returns_CorrectString(long bytes, string expected)
        {
            var result = BytesToStringConverter.Convert(bytes);
            Assert.Equal(expected, result);
        }
    }
}
