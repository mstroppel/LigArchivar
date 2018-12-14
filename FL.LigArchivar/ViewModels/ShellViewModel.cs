using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Caliburn.Micro;
using FL.LigArchivar.Core;
using FL.LigArchivar.ViewModels.Data;

namespace FL.LigArchivar.ViewModels
{
    public class ShellViewModel : Screen
    {
        private static readonly ILog Log = LogManager.GetLog(typeof(ShellViewModel));
        private string _currentRootDirectorySearched;

        public ShellViewModel()
        {
            RootDirectory = Properties.Settings.Default.RootDirectory;
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

        public IImmutableList<ITreeViewItem> Root
        {
            get
            {
                return _root;
            }

            private set
            {
                _root = value;
                NotifyOfPropertyChange(nameof(Root));
            }
        }

        private IImmutableList<ITreeViewItem> _root;

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

                var root = await Task.Run(() => GetArchiveRoot(newPath)).ConfigureAwait(false);

                if (root == null)
                    return;

                // If another check was started, this one is no longer valid.
                if (_currentRootDirectorySearched != newPath)
                    return;

                Log.Info("New directory " + newPath + " exists!");
                SaveRootDirectory();

                var rootItem = new ArchiveRootTreeViewItem(root);
                Root = rootItem.Children;
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
