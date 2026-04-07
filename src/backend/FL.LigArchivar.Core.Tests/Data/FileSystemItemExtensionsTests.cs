using FL.LigArchivar.Core.Data;
using System.IO.Abstractions;
using Xunit;

namespace FL.LigArchivar.Core.Tests.Data;

public class FileSystemItemExtensionsTests
{
    private readonly ArchiveRoot _root;

    public FileSystemItemExtensionsTests()
    {
        IFileSystem fs = FileSystemForTesting.Create();
        var created = ArchiveRoot.TryCreate("/archiv", fs, out _root!);
        Assert.True(created);
        Assert.NotNull(_root);
    }

    [Fact]
    public void GetYear_ReturnsCorrectYear()
    {
        var child = _root.GetChild("Digitalfoto/2018/A-Albverein");

        var actualYear = child!.GetYear();

        Assert.Equal("2018", actualYear);
    }

    [Fact]
    public void GetClubChar_ReturnsCorrectChar()
    {
        var child = _root.GetChild("Digitalfoto/2018/A-Albverein");

        var actualClubChar = child!.GetClubChar();

        Assert.Equal("A", actualClubChar);
    }

    [Fact]
    public void IsInPictures_ReturnsTrueForDigitalfoto()
    {
        var child = _root.GetChild("Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung");

        Assert.True(child!.IsInPictures());
    }
}
