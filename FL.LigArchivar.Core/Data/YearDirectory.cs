using System.Collections.Immutable;
using System.IO;

namespace FL.LigArchivar.Core.Data
{
    public class YearDirectory : FileSystemItemBase
    {
        private YearDirectory(DirectoryInfo yearDirectory)
            : base(yearDirectory, yearDirectory.Name, true, null)
        {
        }

        public static bool TryCreate(DirectoryInfo yearDirectory, out IFileSystemItem directory)
        {
            directory = null;
            var name = yearDirectory.Name;

            if (!int.TryParse(name, out var year))
                return false;

            // Des kaa beim beschta Willa it sei.
            var under1700 = year < 1700;
            var over3000 = year > 3000;
            if (under1700 || over3000)
                return false;

            directory = new YearDirectory(yearDirectory);
            return true;
        }
    }
}
