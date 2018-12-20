using System.Collections.Immutable;
using System.IO;
using System.Linq;
using FL.LigArchivar.Core.Utilities;

namespace FL.LigArchivar.Core.Data
{
    public class AssetDirectory : FileSystemItemBase
    {
        private const string _folderStructureFolderName = @"Ordnerstruktur";

        private static readonly IImmutableList<string> _allowedNames = new[]
        {
            @"Digitalfoto",
            @"Ton",
            @"Video"
        }.ToImmutableList();

        private AssetDirectory(DirectoryInfo assetDirectory)
            : base(assetDirectory, assetDirectory.Name, true, TryCreateChild)
        {
        }

        public static bool TryCreate(DirectoryInfo assetDirectory, out IFileSystemItem directory)
        {
            directory = null;

            if (!DirectoryEx.Exists(assetDirectory.FullName))
                return false;

            var name = assetDirectory.Name;

            if (_allowedNames.All(item => item != name))
                return false;

            directory = new AssetDirectory(assetDirectory);
            return true;
        }

        private static bool TryCreateChild(DirectoryInfo directory, out IFileSystemItem fileSystemItem)
        {
            var isYear = YearDirectory.TryCreate(directory, out fileSystemItem);
            if (isYear)
                return true;

            if (directory.Name == _folderStructureFolderName)
            {
                fileSystemItem = new IgnoredFileSystemItem(directory);
                return true;
            }

            return false;
        }
    }
}
