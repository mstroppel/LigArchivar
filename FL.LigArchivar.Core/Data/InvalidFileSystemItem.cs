using System.IO;

namespace FL.LigArchivar.Core.Data
{
    public class InvalidFileSystemItem : FileSystemItemBase
    {
        private const string InvalidPrefix = "<invalid>";

        public InvalidFileSystemItem(DirectoryInfo directory)
            : base(directory, InvalidPrefix + directory.Name, false)
        {
        }
    }
}
