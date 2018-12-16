using System.Collections.Immutable;
using System.IO;
using System.Linq;
using FL.LigArchivar.Core.Utilities;

namespace FL.LigArchivar.Core.Data
{
    public class AssetDirectory : FileSystemItemBase
    {
        private static readonly IImmutableList<string> _allowedNames = new[]
        {
            @"Digitalfoto",
            @"Ton",
            @"Video"
        }.ToImmutableList();

        private AssetDirectory(DirectoryInfo assetDirectory)
            : base(assetDirectory, assetDirectory.Name, true, YearDirectory.TryCreate)
        {
        }

        public static bool TryCreate(DirectoryInfo assetDirectory, out IFileSystemItem directory)
        {
            directory = null;

            if (!DirectoryEx.Exists(assetDirectory.FullName))
                return false;

            var name = assetDirectory.Name;

            if (!_allowedNames.Any(item => item == name))
                return false;

            directory = new AssetDirectory(assetDirectory);
            return true;
        }
    }
}
