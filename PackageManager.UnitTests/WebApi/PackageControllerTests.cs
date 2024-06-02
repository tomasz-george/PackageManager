using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PackageManager.Application.Interfaces;
using PackageManager.WebApi.Controllers;
using System.Text;

namespace YourNamespace.Tests
{
    public class PackageControllerTests
    {
        private readonly Mock<IPackageValidationService> _mockPackageValidationService;
        private readonly Mock<ILogger<PackageController>> _mockLogger;
        private readonly PackageController _controller;

        public PackageControllerTests()
        {
            _mockPackageValidationService = new Mock<IPackageValidationService>();
            _mockLogger = new Mock<ILogger<PackageController>>();
            _controller = new PackageController(_mockLogger.Object, _mockPackageValidationService.Object);
        }

        [Fact]
        public async Task UploadFile_ReturnsBadRequest_WhenFileIsNull()
        {
            // Arrange
            IFormFile file = null;

            // Act
            var result = await _controller.UploadFile(file);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No file uploaded.", badRequestResult.Value);
        }

        [Fact]
        public async Task UploadFile_ReturnsBadRequest_WhenFileIsEmpty()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);

            // Act
            var result = await _controller.UploadFile(fileMock.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No file uploaded.", badRequestResult.Value);
        }

        [Fact]
        public async Task UploadFile_ReturnsFileResult_WhenFileIsValid()
        {
            // Arrange
            var content = "1\nA,1\n1\nA,1,B,1";
            var fileName = "input001.txt";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(stream.Length);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.ContentType).Returns("text/plain");
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

            _mockPackageValidationService.Setup(s => s.IsInstallationValid(It.IsAny<string[]>())).Returns(true);
            _mockPackageValidationService.Setup(s => s.ProduceResponseContent(It.IsAny<bool>())).Returns("PASS");

            // Act
            var result = await _controller.UploadFile(fileMock.Object);

            // Assert
            var fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("output001.txt", fileResult.FileDownloadName);

            using (var memoryStream = new MemoryStream())
            {
                await fileResult.FileStream.CopyToAsync(memoryStream);
                var resultContent = Encoding.UTF8.GetString(memoryStream.ToArray());
                Assert.Equal("PASS", resultContent);
            }
        }

        [Fact]
        public async Task UploadFile_ReturnsUnsupportedMediaType_WhenFileTypeIsNotTextPlain()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.ContentType).Returns("application/json");

            // Act
            var result = await _controller.UploadFile(fileMock.Object);

            // Assert
            var unsupportedMediaTypeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status415UnsupportedMediaType, unsupportedMediaTypeResult.StatusCode);
            Assert.Equal("Unsupported media type.", unsupportedMediaTypeResult.Value);
        }
    }
}