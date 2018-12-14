using System.Collections.Immutable;

namespace FL.LigArchivar.ViewModels.Data
{
    internal abstract class TreeViewItemBase : ITreeViewItem
    {
        private ITreeViewItem _parent;

        protected TreeViewItemBase(string name, ITreeViewItem parent = null)
        {
            Name = name;
            _parent = parent;
        }

        public string Name { get; }

        public string FullPath
        {
            get
            {
                if (_parent == null)
                    return Name;

                return _parent.FullPath + "." + Name;
            }
        }

        public abstract IImmutableList<ITreeViewItem> Children { get; }
    }
}
