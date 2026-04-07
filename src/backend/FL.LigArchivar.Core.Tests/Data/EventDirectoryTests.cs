using FL.LigArchivar.Core.Data;
using System.IO.Abstractions;
using Xunit;

namespace FL.LigArchivar.Core.Tests.Data;

public class EventDirectoryTests
{
    private readonly ArchiveRoot _root;

    public EventDirectoryTests()
    {
        IFileSystem fs = FileSystemForTesting.Create();
        var created = ArchiveRoot.TryCreate("/archiv", fs, out _root!);
        Assert.True(created);
        Assert.NotNull(_root);
    }

    [Fact]
    public void TryCreate_ParsesEventDirectoryCorrectly()
    {
        // Arrange & Act
        var uut = _root.GetChild("Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung");

        // Assert
        var eventDir = Assert.IsType<EventDirectory>(uut);
        Assert.Equal("A", eventDir.ClubChar);
        Assert.Equal("2018", eventDir.Year);
        Assert.Equal("05", eventDir.Month);
        Assert.Equal("01", eventDir.Day);
        Assert.Equal("Maiwanderung", eventDir.EventName);
    }

    [Fact]
    public void LoadChildren_ReturnsCorrectFileCount()
    {
        // Arrange
        var uut = _root.GetChild("Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung")
            as EventDirectory;
        Assert.NotNull(uut);

        // Act
        uut.LoadChildren();

        // Assert
        Assert.Equal(5, uut.Children.Count);
    }

    [Fact]
    public void Rename_RenumbersFilesFromGivenStart()
    {
        // Arrange
        var uut = _root.GetChild("Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung")
            as EventDirectory;
        Assert.NotNull(uut);
        uut.LoadChildren();

        // Act
        uut.Rename(10);

        // Assert
        Assert.Equal(5, uut.Children.Count);
        Assert.Equal("A_2018-05-01_010", uut.Children[0].Name);
        Assert.Equal("A_2018-05-01_014", uut.Children[4].Name);
    }
}
