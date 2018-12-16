using System.Collections.Immutable;
using System.Linq;
using FL.LigArchivar.Core.Data;

namespace FL.LigArchivar.ViewModels.Data
{
    internal class FilesSystemTreeViewItem : TreeViewItemBase
    {
        private readonly IFileSystemItem _inner;

        public FilesSystemTreeViewItem(IFileSystemItem inner, ITreeViewItem parent = null)
            : base(inner.Name, parent, inner.IsValid)
        {
            _inner = inner;
            Children = inner.Children
                .Select(item => item.ToTreeViewItem(this))
                .ToImmutableList();
        }

        public override IImmutableList<ITreeViewItem> Children { get; }
    }
}
