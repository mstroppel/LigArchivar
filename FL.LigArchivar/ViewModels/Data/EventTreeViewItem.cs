using System.Collections.Immutable;
using FL.LigArchivar.Core.Data;

namespace FL.LigArchivar.ViewModels.Data
{
    public class EventTreeViewItem : TreeViewItemBase
    {
        private readonly EventDirectory _inner;

        public EventTreeViewItem(EventDirectory inner, ITreeViewItem parent)
            : base(inner.Name, parent, inner.IsValid)
        {
            _inner = inner;
        }

        public string FilePrefix => _inner.FilePrefix;

        public override IImmutableList<ITreeViewItem> Children { get; } = ImmutableList<ITreeViewItem>.Empty;
    }
}
