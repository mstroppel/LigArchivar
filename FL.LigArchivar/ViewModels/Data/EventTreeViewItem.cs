using FL.LigArchivar.Core.Data;
using System.Collections.Immutable;
using System.Linq;

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

        public IImmutableList<FileListItem> Children
        {
            get => _children;
            set
            {
                if (_children != value)
                {
                    _children = value;
                    NotifyOfPropertyChange(nameof(Children));
                }
            }
        }

        private IImmutableList<FileListItem> _children = ImmutableList<FileListItem>.Empty;

        internal void LoadChildren()
        {
            _inner.LoadChildren();
            Children = _inner.Children
                .Select(item => new FileListItem(item))
                .ToImmutableList();
        }
    }
}
