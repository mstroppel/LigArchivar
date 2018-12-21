using Caliburn.Micro;

namespace FL.LigArchivar.ViewModels.Data
{
    public abstract class TreeViewItemBase : PropertyChangedBase, ITreeViewItem
    {
        private const string PathSeparationString = ".";
        private ITreeViewItem _parent;

        protected TreeViewItemBase(string name, ITreeViewItem parent = null, bool isValid = true)
        {
            Name = name;
            _parent = parent;
            if (parent == null)
                FullPath = name;
            else
                FullPath = parent.FullPath + PathSeparationString + name;
            IsValid = isValid;
        }

        public string Name { get; }

        public string FullPath { get; }

        public bool IsValid { get; }
    }
}
