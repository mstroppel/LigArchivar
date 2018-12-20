using System.IO;

namespace FL.LigArchivar.Core.Data
{
    public class InvalidFileSystemItem : FileSystemItemBase
    {
        public InvalidFileSystemItem(DirectoryInfo directory, IFileSystemItem parent)
            : base(directory, directory.Name, parent, false)
        {
        }
    }
}
