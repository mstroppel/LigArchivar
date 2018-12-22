using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Caliburn.Micro;
using FL.LigArchivar.Core;
using FL.LigArchivar.Core.Data;
using FL.LigArchivar.MessageBox;
using FL.LigArchivar.ViewModels.Data;

namespace FL.LigArchivar.ViewModels
{
    public class ShellViewModel : Screen
    {
        private static readonly ILog Log = LogManager.GetLog(typeof(ShellViewModel));
        private readonly IMessageBox _messageBox;
        private string _currentRootDirectorySearched;

        public ShellViewModel(IMessageBox messageBox)
        {
            RootDirectory = Properties.Settings.Default.RootDirectory;
            _messageBox = messageBox;
        }

        public string RootDirectory
        {
            get { return _rootDirectory; }
            set
            {
                if (_rootDirectory != value)
                {
                    _rootDirectory = value;
                    NotifyOfPropertyChange(nameof(RootDirectory));
                    OnRootDirectoryChanged(value);
                }
            }
        }

        private string _rootDirectory;

        private ArchiveRootTreeViewItem Root
        {
            get => _root;
            set
            {
                if (_root != value)
                {
                    _root = value;
                    NotifyOfPropertyChange(nameof(Root));
                    if (_root != null)
                        RootChildren = _root.Children;
                    else
                        RootChildren = ImmutableList<ITreeViewItem>.Empty;
                }
            }
        }

        private ArchiveRootTreeViewItem _root = null;

        public IImmutableList<ITreeViewItem> RootChildren
        {
            get { return _rootChildren; }
            private set
            {
                _rootChildren = value;
                NotifyOfPropertyChange(nameof(RootChildren));
            }
        }

        private IImmutableList<ITreeViewItem> _rootChildren;

        internal object SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    NotifyOfPropertyChange(nameof(SelectedItem));
                    var asEvent = value as EventTreeViewItem;
                    Event = asEvent;
                }
            }
        }

        private object _selectedItem;

        public EventTreeViewItem Event
        {
            get => _event;
            set
            {
                if (_event != value)
                {
                    _event = value;
                    if (value != null)
                    {
                        value.LoadChildren();
                        IgnoreWhereNoJPEG = value.IsInPictures;
                    }

                    NotifyOfPropertyChange(nameof(Event));
                }
            }
        }

        private EventTreeViewItem _event;

        public void SortByName()
        {
            var local = Event;
            if (local == null)
                return;

            local.SortByName();
        }

        public void SortByDate()
        {
            var local = Event;
            if (local == null)
                return;

            local.SortByDate();
        }

        public bool IgnoreWhereNoJPEG
        {
            get => _ignoreWhereNoJPEG;
            set
            {
                if (_ignoreWhereNoJPEG != value)
                {
                    _ignoreWhereNoJPEG = value;
                    NotifyOfPropertyChange(nameof(IgnoreWhereNoJPEG));
                }
            }
        }

        private bool _ignoreWhereNoJPEG;

        public void Rename()
        {
            var local = Event;
            if (local == null)
                return;

            local.Rename(IgnoreWhereNoJPEG);
        }

        private static ArchiveRoot GetArchiveRoot(string rootDirectoryPath)
        {
            if (ArchiveRoot.TryCreate(rootDirectoryPath, out var root))
                return root;

            return null;
        }

        private async void OnRootDirectoryChanged(string newPath)
        {
            try
            {
                _currentRootDirectorySearched = newPath;

                var archiveRoot = await Task.Run(() => GetArchiveRoot(newPath)).ConfigureAwait(false);

                if (archiveRoot == null)
                {
                    Root = null;
                    return;
                }

                // If another check was started, this one is no longer valid.
                if (_currentRootDirectorySearched != newPath)
                    return;

                Log.Info("New directory " + newPath + " exists!");
                SaveRootDirectory();

                var rootItem = new ArchiveRootTreeViewItem(archiveRoot, _messageBox);
                Root = rootItem;
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to open the directory '{0}'. Exception details follow.", newPath);
                Log.Error(ex);
            }
        }

        private void SaveRootDirectory()
        {
            // Save the new file path of the settings.
            Properties.Settings.Default.RootDirectory = _rootDirectory;
            Properties.Settings.Default.Save();
        }
    }
}
