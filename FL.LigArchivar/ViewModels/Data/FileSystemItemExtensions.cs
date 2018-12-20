using FL.LigArchivar.Core.Data;

namespace FL.LigArchivar.ViewModels.Data
{
    internal static class FileSystemItemExtensions
    {
        public static ITreeViewItem ToTreeViewItem(this IFileSystemItem item, ITreeViewItem parent)
        {
            var asEventDirectory = item as EventDirectory;
            if (asEventDirectory != null)
                return new EventTreeViewItem(asEventDirectory, parent);

            return new FilesSystemTreeViewItem(item, parent);
        }
    }
}
