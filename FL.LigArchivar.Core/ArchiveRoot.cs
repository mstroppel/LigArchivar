using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using FL.LigArchivar.Core.Data;
using FL.LigArchivar.Core.Utilities;

namespace FL.LigArchivar.Core
{
    public class ArchiveRoot
    {
        private readonly string _directoryPath;
        private readonly DirectoryInfo _directoryInfo;

        private ArchiveRoot(string archiveRootDirectoryPath)
        {
            _directoryPath = archiveRootDirectoryPath;
            _directoryInfo = new DirectoryInfo(archiveRootDirectoryPath);
            Children = GetChildren(_directoryInfo);
        }

        public static bool TryCreate(string archiveRootDirectoryPath, out ArchiveRoot archivar)
        {
            archivar = null;

            if (!DirectoryEx.Exists(archiveRootDirectoryPath))
                return false;

            archivar = new ArchiveRoot(archiveRootDirectoryPath);
            return true;
        }

        public IImmutableList<IFileSystemItem> Children { get; }

        private static IImmutableList<IFileSystemItem> GetChildren(DirectoryInfo directory)
        {
            var items = new List<IFileSystemItem>();

            var subDirectories = directory.GetDirectories();
            foreach (var subDirectory in subDirectories)
            {
                AssetDirectory asset;
                if (AssetDirectory.TryCreate(subDirectory, out asset))
                {
                    items.Add(asset);
                }
                else
                {
                    var invalidItem = new InvalidFileSystemItem(subDirectory.Name);
                    items.Add(invalidItem);
                }
            }

            return items.ToImmutableList();
        }
    }
}
