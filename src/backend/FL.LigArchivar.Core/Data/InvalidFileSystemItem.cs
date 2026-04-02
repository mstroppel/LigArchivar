using System.Collections.Immutable;

using System.IO.Abstractions;

namespace FL.LigArchivar.Core.Data;

public class InvalidFileSystemItem : FileSystemItemBase
{
    public InvalidFileSystemItem(IDirectoryInfo directoryInfo, IFileSystemItem? parent)
        : base(directoryInfo, directoryInfo.Name, parent, false, null)
    {
    }
}