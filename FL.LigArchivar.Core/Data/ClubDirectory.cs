using System.Collections.Immutable;

using System.IO.Abstractions;
using System.Linq;
using FL.LigArchivar.Core.Utilities;

namespace FL.LigArchivar.Core.Data
{
    public class ClubDirectory : FileSystemItemBase
    {
        private static readonly IImmutableList<string> _allowedNames = new[]
        {
            @"A-Albverein",
            @"C-Gemischter_Chor",
            @"D-Dorffest",
            @"F-Film-Liga",
            @"G-Gemeinde",
            @"H-Hochzeiten",
            @"K-Kirche",
            @"L-Ledige",
            @"M-Musikverein",
            @"N-Narrenzunft",
            @"P-Personen_und_Begebenheiten",
            @"S-Schuetzenverein",
            @"T-TSV",
            @"V-Ansichten",
            @"W-Feuerwehr",
            @"Z-Kegler"
        }.ToImmutableList();

        private ClubDirectory(DirectoryInfoBase assetDirectory, IFileSystemItem parent)
            : base(assetDirectory, assetDirectory.Name, parent, true)
        {
        }

        public static bool TryCreate(DirectoryInfoBase assetDirectory, IFileSystemItem parent, out IFileSystemItem directory)
        {
            directory = null;

            if (!DirectoryEx.Exists(assetDirectory.FullName))
                return false;

            var name = assetDirectory.Name;

            if (_allowedNames.All(item => item != name))
                return false;

            directory = new ClubDirectory(assetDirectory, parent);
            return true;
        }
    }
}