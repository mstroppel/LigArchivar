using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace FL.LigArchivar.Core.Tests;

public class ArchiveRootTests
{
    [Fact]
    public void TryCreate_ReturnsTrue_WhenPathExists()
    {
        // Arrange
        var fs = new MockFileSystem();
        fs.AddDirectory("/archiv");

        // Act
        var result = ArchiveRoot.TryCreate("/archiv", fs, out var archivar);

        // Assert
        result.Should().BeTrue();
        archivar.Should().NotBeNull();
    }

    [Fact]
    public void TryCreate_ReturnsFalse_WhenPathDoesNotExist()
    {
        // Arrange
        var fs = new MockFileSystem();

        // Act
        var result = ArchiveRoot.TryCreate("/nonexistent", fs, out var archivar);

        // Assert
        result.Should().BeFalse();
        archivar.Should().BeNull();
    }

    [Fact]
    public void TryCreate_ReturnsFalse_WhenPathIsAFile()
    {
        // Arrange
        var fs = new MockFileSystem();
        fs.AddFile("/archiv", new MockFileData(""));

        // Act
        var result = ArchiveRoot.TryCreate("/archiv", fs, out var archivar);

        // Assert
        result.Should().BeFalse();
        archivar.Should().BeNull();
    }
}
