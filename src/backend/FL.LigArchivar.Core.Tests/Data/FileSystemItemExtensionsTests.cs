using FL.LigArchivar.Core.Data;
using Xunit;

namespace FL.LigArchivar.Core.Tests.Data;

public class FileSystemItemExtensionsTests
{
    private readonly ArchiveRoot _root;

    public FileSystemItemExtensionsTests()
    {
        var fs = FileSystemForTesting.Create();
        var created = ArchiveRoot.TryCreate("/archiv", fs, out _root!);
        created.Should().BeTrue();
        _root.Should().NotBeNull();
    }

    [Fact]
    public void GetYear_ReturnsCorrectYear()
    {
        var child = _root.GetChild("Digitalfoto/2018/A-Albverein");

        child!.GetYear().Should().Be("2018");
    }

    [Fact]
    public void GetClubChar_ReturnsCorrectChar()
    {
        var child = _root.GetChild("Digitalfoto/2018/A-Albverein");

        child!.GetClubChar().Should().Be("A");
    }

    [Fact]
    public void IsInPictures_ReturnsTrueForDigitalfoto()
    {
        var child = _root.GetChild("Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung");

        child!.IsInPictures().Should().BeTrue();
    }
}
