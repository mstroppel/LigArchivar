using System;
using System.Collections.Immutable;
using System.Linq;
using Caliburn.Micro;
using FL.LigArchivar.Core.Data;

namespace FL.LigArchivar.ViewModels.Data
{
    public class EventTreeViewItem : TreeViewItemBase
    {
        private static readonly ILog _log = LogManager.GetLog(typeof(TreeViewItemBase));
        private readonly EventDirectory _inner;

        public EventTreeViewItem(EventDirectory inner, ITreeViewItem parent)
            : base(inner.Name, parent, inner.IsValid)
        {
            _inner = inner;
        }

        public string FilePrefix => _inner.FilePrefix;

        public IImmutableList<FileListItem> Files
        {
            get => _files;
            set
            {
                if (_files != value)
                {
                    _files = value;
                    NotifyOfPropertyChange(nameof(Files));
                }
            }
        }

        public bool IsInPictures => _inner.IsInPictures();

        public override IImmutableList<ITreeViewItem> Children { get; } = ImmutableList<ITreeViewItem>.Empty;

        private IImmutableList<FileListItem> _files = ImmutableList<FileListItem>.Empty;

        internal void LoadChildren()
        {
            _inner.LoadChildren();
            UpdateFilesFromInner();
        }

        internal void SortByName()
        {
            _inner.SortByName();
            UpdateFilesFromInner();
        }

        internal void SortByDate()
        {
            _inner.SortByDate();
            UpdateFilesFromInner();
        }

        internal void Rename(int startNumber)
        {
            try
            {
                _inner.Rename(startNumber);
            }
            catch (Exception e)
            {
                _log.Error(e);

                var root = GetRoot();
                if (root == null)
                    return;

                root.MessageBox.ShowException(e);
            }

            UpdateFilesFromInner();
        }

        private void UpdateFilesFromInner()
        {
            Files = _inner.Children
                .Where(item => !item.IsIgnored)
                .Select(item => new FileListItem(item))
                .ToImmutableList();
        }
    }
}
