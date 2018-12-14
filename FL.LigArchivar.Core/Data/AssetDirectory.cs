using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using FL.LigArchivar.Core.Utilities;

namespace FL.LigArchivar.Core.Data
{
    public class AssetDirectory : PropertyChangedBase, IFileSystemItem
    {
        private static readonly IImmutableList<string> _allowedNames = new[]
        {
            @"Digitalfoto",
            @"Ton",
            @"Video"
        }.ToImmutableList();
        private readonly DirectoryInfo _assetDirectory;

        public AssetDirectory(DirectoryInfo assetDirectory)
        {
            _assetDirectory = assetDirectory;
            Name = assetDirectory.Name;
        }

        public static bool TryCreate(DirectoryInfo assetDirectory, out AssetDirectory directory)
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

        public string Name { get; }

        public bool Valid => true;
    }
}
