using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FL.LigArchivar.Core.Data
{
    internal static class FileSystemItemExtensions
    {
        public static string GetYear(this IFileSystemItem self)
        {
            if (self == null)
                return null;

            var selfAsYear = self as YearDirectory;
            if (selfAsYear != null)
                return selfAsYear.Name;

            var year = GetYear(self.Parent);
            return year;
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
            var name = path.FirstOrDefault();
            if (name == null)
                return self as IFileSystemItem;

            var child = self.Children
                .FirstOrDefault(item => item.Name == name);

            var nextPaths = path.Skip(1).ToImmutableList();
            if (nextPaths.IsEmpty)
                return child as IFileSystemItem;

            var childWithChildren = child as IFileSystemItemWithChildren;
            if (childWithChildren != null)
                child = childWithChildren.GetChild(nextPaths);

            return child;
        }
    }
}
