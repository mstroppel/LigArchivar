using System.Collections.Immutable;
using FL.LigArchivar.Core.Data;

namespace FL.LigArchivar.ViewModels.Data
{
    internal class FilesSystemTreeViewItem : TreeViewItemBase
    {
        private readonly IFileSystemItem _inner;

        public FilesSystemTreeViewItem(IFileSystemItem inner, ITreeViewItem parent = null)
            : base(inner.Name, parent)
        {
            _inner = inner;
            Children = ImmutableList<ITreeViewItem>.Empty;
        }

        public override IImmutableList<ITreeViewItem> Children { get; }
    }
}
