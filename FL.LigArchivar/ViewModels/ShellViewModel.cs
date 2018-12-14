using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using FL.LigArchivar.Core.Utilities;

namespace FL.LigArchivar.ViewModels
{
    /// <summary>
    /// View model for the main shell.
    /// </summary>
    /// <seealso cref="Screen" />
    public class ShellViewModel : Screen
    {
        private static readonly ILog Log = LogManager.GetLog(typeof(ShellViewModel));
        private string _currentRootDirectorySearched;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
        /// </summary>
        public ShellViewModel()
        {
            RootDirectory = Properties.Settings.Default.RootDirectory;
        }

        /// <summary>
        /// Gets or sets the root directory.
        /// </summary>
        /// <value>
        /// The root directory.
        /// </value>
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

        /// <summary>
        /// Called when <see cref="RootDirectory"/> has changed.
        /// </summary>
        /// <param name="newPath">The new path.</param>
        private async void OnRootDirectoryChanged(string newPath)
        {
            try
            {
                _currentRootDirectorySearched = newPath;

                var exists = await Task.Run(() => DirectoryEx.Exists(newPath)).ConfigureAwait(false);

                if (!exists)
                    return;

                // If another check was started, this one is no longer valid.
                if (_currentRootDirectorySearched != newPath)
                    return;

                Log.Info("New directory " + newPath + " exists!");
                SaveRootDirectory();
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to open the directory '{0}'. Exception details follow.", newPath);
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Saves the currently selected root directory to the user settings of the application.
        /// </summary>
        private void SaveRootDirectory()
        {
            // Save the new file path of the settings.
            Properties.Settings.Default.RootDirectory = _rootDirectory;
            Properties.Settings.Default.Save();
        }
    }
}
