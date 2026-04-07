using System.Collections.Immutable;
using System.IO.Abstractions;
using FL.LigArchivar.Core.Data;
using FL.LigArchivar.Core.Utilities;

namespace FL.LigArchivar.Core;

public class ArchiveRoot : IFileSystemItemWithChildren
{
    private readonly IDirectoryInfo _directoryInfo;

    private ArchiveRoot(string archiveRootDirectoryPath, IFileSystem fileSystem)
    {
        _directoryInfo = fileSystem.DirectoryInfo.New(archiveRootDirectoryPath);
        Children = _directoryInfo.GetChildrenFileSystemItems(null, AssetDirectory.TryCreate);
    }

    public IImmutableList<IFileSystemItem> Children { get; }

    public static bool TryCreate(string archiveRootDirectoryPath, IFileSystem fileSystem, out ArchiveRoot? archivar)
    {
        archivar = null;

        if (!DirectoryEx.Exists(fileSystem, archiveRootDirectoryPath))
            return false;

        archivar = new ArchiveRoot(archiveRootDirectoryPath, fileSystem);
        return true;
    }
}
