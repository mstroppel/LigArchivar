using FL.LigArchivar.ViewModels;
using WPFFolderBrowser;

namespace FL.LigArchivar.Views
{
    /// <summary>
    /// Interaction logic of <see cref="ShellView"/>.
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class ShellView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShellView"/> class.
        /// </summary>
        public ShellView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the BrowseRootDirectory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BrowseRootDirectory_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var dialog = new WPFFolderBrowserDialog();
            dialog.InitialDirectory = RootDirectory.Text;

            var result = dialog.ShowDialog();
            if (result == false)
            {
                return;
            }

            RootDirectory.Text = dialog.FileName;
        }

        private void RootChildren_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            var viewModel = DataContext as ShellViewModel;
            viewModel.SelectedItem = e.NewValue;
        }
    }
}
