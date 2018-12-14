using Caliburn.Micro;

namespace FL.LigArchivar.ViewModels
{
    /// <summary>
    /// View model for the main shell.
    /// </summary>
    /// <seealso cref="Screen" />
    public class ShellViewModel : Screen
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
        /// </summary>
        public ShellViewModel()
        {
            _rootDirectory = Properties.Settings.Default.RootDirectory;
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
                _rootDirectory = value;
                NotifyOfPropertyChange(nameof(RootDirectory));
            }
        }

        private string _rootDirectory;
    }
}
