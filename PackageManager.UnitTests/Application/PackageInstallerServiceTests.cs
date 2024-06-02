using Microsoft.Extensions.Logging;
using Moq;
using PackageManager.Application.Interfaces;
using PackageManager.Application.Services;

namespace PackageManager.UnitTests.Application
{
    public class PackageManagerServiceTests
    {
        private readonly IPackageValidationService _packageValidationService;
        private readonly Mock<ILogger<PackageValidationService>> _loggerMock;

        public PackageManagerServiceTests()
        {
            _loggerMock = new Mock<ILogger<PackageValidationService>>();
            _packageValidationService = new PackageValidationService(_loggerMock.Object);
        }

        #region Sample input data
        [Theory]
        [InlineData("1", "P1,42", "1", "P1,42,P2,Beta-1")]
        [InlineData("1", "B,1", "1", "B,1,B,1")]
        [InlineData("2", "A,2", "B,2", "3", "A,1, B,1", "A,1, B,2", "A,2, C,3")]
        [InlineData("1", "B,2", "2", "A,1,B,2", "B,2,A,1")]
        [InlineData("1", "A,1")]
        [InlineData("3", "A,2", "B,2", "G,1", "5", "A,1,B,2", "A,2,C,3", "C,3,D,4", "D,4,G,1", "G,1,B,2")]
        public void IsInstallationValid_TestData_ReturnsTrue(params string[] installationFile)
        {
            // Act
            bool result = _packageValidationService.IsInstallationValid(installationFile);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("1", "A,1", "2", "A,1,B,2", "A,1,B,1")]
        [InlineData("1", "B,2", "2", "B,2,A,1,C,1", "C,1,A,2")]
        [InlineData("2", "A,2", "B,2", "5", "A,1,B,1", "A,1,B,2", "A,2,C,3", "C,3,D,4", "D,4,B,1")]
        [InlineData("2", "A,1", "C,1", "2", "A,1,B,1", "C,1,B,2")]
        public void IsInstallationValid_TestData_ReturnsFalse(params string[] installationFile)
        {
            // Act
            bool result = _packageValidationService.IsInstallationValid(installationFile);

            // Assert
            Assert.False(result);
        }
        #endregion


        [Fact]
        public void IsInstallationValid_SinglePackageNoDependencies_ReturnsTrue()
        {
            // Arrange
            string[] input =
            [
            "1",
            "A,1",
            "0"
            ];

            // Act
            bool result = _packageValidationService.IsInstallationValid(input);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsInstallationValid_ConflictingPackageVersions_ReturnsFalse()
        {
            // Arrange
            string[] input =
            [
            "2",
            "A,1",
            "B,2",
            "2",
            "A,1,B,1",
            "B,2,C,1"
            ];

            // Act
            bool result = _packageValidationService.IsInstallationValid(input);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsInstallationValid_ConflictingPackagesWithoutDependencies_ReturnsFalse()
        {
            // Arrange
            string[] input =
            [
            "2",
            "A,1",
            "B,2",
            "A,2"
            ];

            // Act
            bool result = _packageValidationService.IsInstallationValid(input);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsInstallationValid_InvalidPackageFormat_ThrowsArgumentException()
        {
            // Arrange
            string[] input =
            [
            "1",
            "A,1,1",  // Invalid format
            "0"
            ];

            // Act
            bool result = _packageValidationService.IsInstallationValid(input);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsInstallationValid_MultipleDependenciesConflict_ReturnsFalse()
        {
            // Arrange
            string[] input =
            [
            "1",
            "B,2",
            "2",
            "B,2,A,1,C,1",
            "C,1,A,2"
            ];

            // Act
            bool result = _packageValidationService.IsInstallationValid(input);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsInstallationValid_NoDependencies_ReturnsTrue()
        {
            // Arrange
            string[] input =
            [
            "1",
            "A,1",
            "0"
            ];

            // Act
            bool result = _packageValidationService.IsInstallationValid(input);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsInstallationValid_EmptyFile_ReturnsFalse()
        {
            // Arrange
            string[] input = [];

            // Act
            bool result = _packageValidationService.IsInstallationValid(input);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsInstallationValid_PackageNumberIncorrect_ReturnsFalse()
        {
            // Arrange
            string[] input =
            [
            "2",
            "B,2",
            "2",
            "B,2,A,1,C,1",
            "C,1,A,2"
            ];

            // Act
            bool result = _packageValidationService.IsInstallationValid(input);

            // Assert
            Assert.False(result);
        }
    }
}