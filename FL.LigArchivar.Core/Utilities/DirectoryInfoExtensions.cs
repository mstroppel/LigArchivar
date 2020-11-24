using System.Collections.Generic;
using System.Collections.Immutable;

using System.IO.Abstractions;
using FL.LigArchivar.Core.Data;

namespace FL.LigArchivar.Core.Utilities
{
    public delegate bool TryCreateFileSystemItem(IDirectoryInfo directory, IFileSystemItem parent, out IFileSystemItem item);

    public static class DirectoryInfoExtensions
    {
        public static IImmutableList<IFileSystemItem> GetChildrenFileSystemItems(this IDirectoryInfo self, IFileSystemItem parent, TryCreateFileSystemItem tryCreateChild)
        {
            var items = new List<IFileSystemItem>();

            var subDirectories = self.GetDirectories();
            foreach (var subDirectory in subDirectories)
            {
                var created = tryCreateChild(subDirectory, parent, out var item);
                if (created)
                {
                    items.Add(item);
                }
                else
                {
                    var invalidItem = new InvalidFileSystemItem(subDirectory, parent);
                    items.Add(invalidItem);
                }
            }

            return items.ToImmutableList();
        }
    }
}
