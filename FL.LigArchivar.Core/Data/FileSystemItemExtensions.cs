using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FL.LigArchivar.Core.Data
{
    internal static class FileSystemItemExtensions
    {
        public static string GetYear(this IFileSystemItem self)
        {
            if (self == null)
                throw new InvalidOperationException("Extension Method GetYear() must be called on a file system item that underneath the YearDirectory.");

            var selfAsYear = self as YearDirectory;
            if (selfAsYear != null)
                return selfAsYear.Name;

            return GetYear(self.Parent);
        }

        public static IFileSystemItem GetChild(this IFileSystemItemWithChildren self, string path)
        {
            var splitted = path.Split('\\');
            var items = splitted
                .Where(item => !string.IsNullOrWhiteSpace(item));

            var child = self.GetChild(items);
            return child;
        }

        public static IFileSystemItem GetChild(this IFileSystemItemWithChildren self, IEnumerable<string> path)
        {
            var name = path.First();

            var child = self.Children
                .FirstOrDefault(item => item.Name == name);

            return child;
        }
    }
}
