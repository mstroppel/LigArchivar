using System.IO;

namespace FL.LigArchivar.Core.Data
{
    public class IgnoredFileSystemItem : FileSystemItemBase
    {
        private const string IgnoredPrefix = "<ignored>";

        public IgnoredFileSystemItem(DirectoryInfo directory)
            : base(directory, IgnoredPrefix + directory.Name, true)
        {
        }
    }
}
