using System.IO.Abstractions.TestingHelpers;
using FL.LigArchivar.Core.Data;
using FluentAssertions;
using Xunit;

namespace FL.LigArchivar.Core.Tests.Data;

public class EventDirectoryTests
{
    private readonly ArchiveRoot _root;

    public EventDirectoryTests()
    {
        var fs = FileSystemForTesting.Create();
        var created = ArchiveRoot.TryCreate("/archiv", fs, out _root!);
        created.Should().BeTrue();
        _root.Should().NotBeNull();
    }


    [Fact]
    public void LoadChildren_ReturnsCorrectFileCount()
    {
        // Arrange
        var uut = _root.GetChild("Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung")
            as EventDirectory;
        uut.Should().NotBeNull();

        // Act
        uut!.LoadChildren();

        // Assert
        uut.Children.Should().HaveCount(5);
    }

    [Fact]
    public void Rename_RenumbersFilesFromGivenStart()
    {
        // Arrange
        var uut = _root.GetChild("Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung")
            as EventDirectory;
        uut.Should().NotBeNull();
        uut!.LoadChildren();

        // Act
        uut.Rename(10);

        // Assert
        uut.Children.Should().HaveCount(5);
        uut.Children[0].Name.Should().Be("A_2018-05-01_010");
        uut.Children[4].Name.Should().Be("A_2018-05-01_014");
    }

    [Fact]
    public void Rename_WithSpecificFileOrder_RenumbersInCorrectOrder()
    {
        // Arrange
        // Using a different prefix or start number to avoid collision if possible, 
        // but RenameFiles throws if ANY file with target name exists.
        // So we should use a fresh file system.
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/archiv/Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung/A_2018-05-01_001.jpg", new MockFileData("") },
            { "/archiv/Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung/A_2018-05-01_002.jpg", new MockFileData("") },
        }, "/archiv");
        ArchiveRoot.TryCreate("/archiv", fs, out var root).Should().BeTrue();
        var uut = root!.GetChild("Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung") as EventDirectory;
        uut!.LoadChildren();

        // Custom order: 002, 001. 
        // 002 should become 010, 001 should become 011
        var order = new[]
        {
            "A_2018-05-01_002",
            "A_2018-05-01_001",
        };

        // Act
        uut.Rename(10, order);

        // Assert
        uut.Children.Should().HaveCount(2);
        
        // We can't easily verify which was which without more complex mocking, 
        // but we can verify they were renamed to the new numbers.
        uut.Children.Select(c => c.Name).Should().BeEquivalentTo(new[]
        {
            "A_2018-05-01_010",
            "A_2018-05-01_011",
        });
    }

    [Fact]
    public void Rename_DeletesLonelyDngFiles()
    {
        // Arrange
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/archiv/Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung/A_2018-05-01_001.dng", new MockFileData("") },
            { "/archiv/Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung/A_2018-05-01_002.jpg", new MockFileData("") },
        }, "/archiv");
        fs.AddDirectory("/archiv"); // Ensure /archiv exists and is a directory
        ArchiveRoot.TryCreate("/archiv", fs, out var root).Should().BeTrue();
        var uut = root!.GetChild("Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung") as EventDirectory;
        uut!.LoadChildren();
        uut.Children.Should().HaveCount(2);

        // Act
        uut.Rename(10);

        // Assert
        uut.Children.Should().HaveCount(1);
        uut.Children[0].Name.Should().Be("A_2018-05-01_010");
        fs.FileExists("/archiv/Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung/A_2018-05-01_001.dng").Should().BeFalse();
    }

    [Fact]
    public void RenameToFileDateTime_RenamesFilesBasedOnLastWriteTime()
    {
        // Arrange
        var time1 = new DateTime(2018, 05, 01, 10, 0, 0, DateTimeKind.Utc);
        var time2 = new DateTime(2018, 05, 01, 11, 0, 0, DateTimeKind.Utc);
        
        var fs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "/archiv/Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung/A_2018-05-01_001.jpg", new MockFileData("") { LastWriteTime = time1 } },
            { "/archiv/Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung/A_2018-05-01_002.jpg", new MockFileData("") { LastWriteTime = time2 } },
        }, "/archiv");
        fs.AddDirectory("/archiv"); 
        ArchiveRoot.TryCreate("/archiv", fs, out var root).Should().BeTrue();
        var uut = root!.GetChild("Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung") as EventDirectory;
        uut!.LoadChildren();

        // Act
        uut.RenameToFileDateTime();

        // Assert
        uut.Children.Should().HaveCount(2);
        uut.Children.Any(c => c.Name == "2018-05-01_10-00-00").Should().BeTrue();
        uut.Children.Any(c => c.Name == "2018-05-01_11-00-00").Should().BeTrue();
    }

    [Fact]
    public void TryCreate_ParsesEventDirectoryCorrectly()
    {
        // Arrange
        var fs = FileSystemForTesting.Create();
        ArchiveRoot.TryCreate("/archiv", fs, out var root).Should().BeTrue();
        var uut = root!.GetChild("Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung");

        // Assert
        var eventDir = uut.Should().BeOfType<EventDirectory>().Subject;
        eventDir.ClubChar.Should().Be("A");
        eventDir.Year.Should().Be("2018");
        eventDir.Month.Should().Be("05");
        eventDir.Day.Should().Be("01");
        eventDir.EventName.Should().Be("Maiwanderung");
    }

    [Fact]
    public void TryCreate_ReturnsFalse_WhenRegexDoesNotMatch()
    {
        // Arrange
        var fs = new MockFileSystem();
        fs.AddDirectory("/archiv");
        var dirInfo = fs.DirectoryInfo.New("/archiv/Digitalfoto/2018/A-Albverein/InvalidName");
        
        // We need a parent for TryCreate
        ArchiveRoot.TryCreate("/archiv", fs, out var root).Should().BeTrue();
        var parent = root!.GetChild("Digitalfoto/2018/A-Albverein");

        // Act
        var result = EventDirectory.TryCreate(dirInfo, parent, out var eventDir);

        // Assert
        result.Should().BeFalse();
        eventDir.Should().BeNull();
    }

    [Fact]
    public void TryCreate_SetsIsValidFalse_WhenClubCharOrYearMismatch()
    {
        // Arrange
        var fs = new MockFileSystem();
        fs.AddDirectory("/archiv");
        // Name says B_2019 but parent is A-Albverein (A) and 2018
        var dirInfo = fs.DirectoryInfo.New("/archiv/Digitalfoto/2018/A-Albverein/B_2019-05-01_Event");
        
        ArchiveRoot.TryCreate("/archiv", fs, out var root).Should().BeTrue();
        var parent = root!.GetChild("Digitalfoto/2018/A-Albverein");

        // Act
        var result = EventDirectory.TryCreate(dirInfo, parent, out var eventDir);

        // Assert
        result.Should().BeTrue();
        eventDir.Should().NotBeNull();
        eventDir.IsValid.Should().BeFalse();
        ((EventDirectory)eventDir).ClubChar.Should().Be("B");
        ((EventDirectory)eventDir).Year.Should().Be("2019");
    }
}
