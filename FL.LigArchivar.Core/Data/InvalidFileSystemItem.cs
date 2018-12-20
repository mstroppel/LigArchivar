using System.IO;

namespace FL.LigArchivar.Core.Data
{
    public class InvalidFileSystemItem : FileSystemItemBase
    {
        public InvalidFileSystemItem(DirectoryInfo directory)
            : base(directory, directory.Name, false)
        {
        }
    }
}
