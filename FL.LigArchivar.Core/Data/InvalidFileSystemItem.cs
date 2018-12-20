using System.IO.Abstractions;

namespace FL.LigArchivar.Core.Data
{
    public class InvalidFileSystemItem : FileSystemItemBase
    {
        public InvalidFileSystemItem(DirectoryInfoBase directory, IFileSystemItem parent)
            : base(directory, directory.Name, parent, false)
        {
        }
    }
}
