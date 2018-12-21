using System.Collections.Immutable;
using System.Linq;
using FL.LigArchivar.Core;
using FL.LigArchivar.MessageBox;

namespace FL.LigArchivar.ViewModels.Data
{
    internal class ArchiveRootTreeViewItem : TreeViewItemBase
    {
        private readonly ArchiveRoot _root;

        public ArchiveRootTreeViewItem(ArchiveRoot root, IMessageBox messageBox)
            : base("<none>")
        {
            _root = root;
            MessageBox = messageBox;
            Children = root.Children
                .Select(item => item.ToTreeViewItem(this))
                .ToImmutableList();
        }

        public IMessageBox MessageBox { get; }

        public override IImmutableList<ITreeViewItem> Children { get; }
    }
}
