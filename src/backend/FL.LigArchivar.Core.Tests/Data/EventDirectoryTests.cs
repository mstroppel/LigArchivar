using FL.LigArchivar.Core.Data;
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
    public void TryCreate_ParsesEventDirectoryCorrectly()
    {
        // Arrange & Act
        var uut = _root.GetChild("Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung");

        // Assert
        var eventDir = uut.Should().BeOfType<EventDirectory>().Subject;
        eventDir.ClubChar.Should().Be("A");
        eventDir.Year.Should().Be("2018");
        eventDir.Month.Should().Be("05");
        eventDir.Day.Should().Be("01");
        eventDir.EventName.Should().Be("Maiwanderung");
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
}
