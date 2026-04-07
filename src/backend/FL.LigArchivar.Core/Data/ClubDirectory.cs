using System.Collections.Immutable;
using System.IO.Abstractions;
using FL.LigArchivar.Core.Utilities;

namespace FL.LigArchivar.Core.Data;

public class ClubDirectory : FileSystemItemBase
{
    private static readonly ImmutableList<string> AllowedNames = ImmutableList.Create(
        "A-Albverein",
        "C-Gemischter_Chor",
        "D-Dorffest",
        "F-Film-Liga",
        "G-Gemeinde",
        "H-Hochzeiten",
        "K-Kirche",
        "L-Ledige",
        "M-Musikverein",
        "N-Narrenzunft",
        "P-Personen_und_Begebenheiten",
        "R-Fischerverein",
        "S-Schuetzenverein",
        "T-TSV",
        "V-Ansichten",
        "W-Feuerwehr",
        "Z-Kegler");

    private ClubDirectory(IDirectoryInfo clubDirectory, IFileSystemItem? parent)
        : base(clubDirectory, clubDirectory.Name, parent, true, EventDirectory.TryCreate)
    {
    }

    public static bool TryCreate(IDirectoryInfo clubDirectory, IFileSystemItem? parent, out IFileSystemItem directory)
    {
        directory = null!;

        if (!DirectoryEx.Exists(clubDirectory.FileSystem, clubDirectory.FullName))
            return false;

        var name = clubDirectory.Name;

        if (AllowedNames.All(item => item != name))
            return false;

        directory = new ClubDirectory(clubDirectory, parent);
        return true;
    }
}
