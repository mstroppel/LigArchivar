using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using FL.LigArchivar.Core.Data;

namespace FL.LigArchivar.Core.Utilities
{
    public delegate bool TryCreateFileSystemItem(DirectoryInfo directory, out IFileSystemItem item);

    public static class DirectoryInfoExtensions
    {
        public static IImmutableList<IFileSystemItem> GetChildrenFileSystemItems(this DirectoryInfo self, TryCreateFileSystemItem tryCreateChild)
        {
            var items = new List<IFileSystemItem>();

            var subDirectories = self.GetDirectories();
            foreach (var subDirectory in subDirectories)
            {
                var created = tryCreateChild(subDirectory, out var item);
                if (created)
                {
                    items.Add(item);
                }
                else
                {
                    var invalidItem = new InvalidFileSystemItem(subDirectory);
                    items.Add(invalidItem);
                }
            }

            return items.ToImmutableList();
        }
    }
}
