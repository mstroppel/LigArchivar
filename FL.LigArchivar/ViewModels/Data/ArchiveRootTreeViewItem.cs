using System.Collections.Immutable;
using System.Linq;
using FL.LigArchivar.Core;

namespace FL.LigArchivar.ViewModels.Data
{
    internal class ArchiveRootTreeViewItem : TreeViewItemBase
    {
        private readonly ArchiveRoot _root;

        public ArchiveRootTreeViewItem(ArchiveRoot root)
            : base("<none>")
        {
            _root = root;
            Children = root.Children
                .Select(item => item.ToTreeViewItem(this))
                .ToImmutableList();
        }

        public override IImmutableList<ITreeViewItem> Children { get; }
    }
}
