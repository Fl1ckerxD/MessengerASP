using CorpNetMessenger.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace CorpNetMessenger.Tests.Services
{
    public class FileServiceTests
    {
        private readonly FileService _fileService;
        private readonly Mock<IFormFileCollection> _mockFileCollection;

        public FileServiceTests()
        {
            _fileService = new FileService();
            _mockFileCollection = new Mock<IFormFileCollection>();
        }

        [Fact]
        public async Task ProcessFiles_WhenFileTooLarge_ThrowsException()
        {
            var largeFile = CreateMockFile("test.jpg", "image/jpeg", 11 * 1024 * 1024);
            SetupFileCollection(largeFile);

            await Assert.ThrowsAsync<ArgumentException>(() =>
            _fileService.ProcessFiles(_mockFileCollection.Object));
        }

        [Fact]
        public async Task ProcessFiles_WithMultipleFiles_ProcessesAllFiles()
        {
            string[] fileNames = { "file2.jpg", "file1.txt" };
            var file1 = CreateMockFile(fileNames[1], "text/plain", 100);
            var file2 = CreateMockFile(fileNames[0], "image/jpeg", 200);
            SetupFileCollection(file1, file2);

            var result = await _fileService.ProcessFiles(_mockFileCollection.Object);

            result.Should().HaveCount(2);
            result.Select(f => f.FileName).Should().Contain(["file1.txt", "file2.jpg"]);
        }

        private IFormFile CreateMockFile(string fileName, string contentType, long length)
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns(contentType);
            mockFile.Setup(f => f.Length).Returns(length);

            return mockFile.Object;
        }

        private void SetupFileCollection(params IFormFile[] files)
        {
            var fileList = files.ToList();
            _mockFileCollection.Setup(f => f.GetEnumerator()).Returns(fileList.GetEnumerator());
            _mockFileCollection.Setup(f => f.Count).Returns(fileList.Count);
        }
    }
}
