using FL.LigArchivar.Core.Data;

namespace FL.LigArchivar.ViewModels.Data
{
    internal static class FileSystemItemExtensions
    {
        public static ITreeViewItem ToTreeViewItem(this IFileSystemItem item, ITreeViewItem parent)
        {
            return new FilesSystemTreeViewItem(item, parent);
        }
    }
}
