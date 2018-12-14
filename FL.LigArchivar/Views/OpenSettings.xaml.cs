using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using FL.LigArchivar.Core.Data;
using Microsoft.Win32;

namespace FL.LigArchivar.Views
{
    /// <summary>
    /// Interaktionslogik für OpenSettingsDialog.xaml
    /// </summary>
    public partial class OpenSettings : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSettings"/> class.
        /// </summary>
        public OpenSettings()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        public AppSettings Settings { get; private set; }

        /// <summary>
        /// Gets or sets the settings file path.
        /// </summary>
        public string SettingsFilePath
        {
            get { return settingsFilePath.Text; }
            set { settingsFilePath.Text = value; }
        }

        /// <summary>
        /// Handles the PreviewDrop event of the settingsFilePath TextBox.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void SettingsFilePath_PreviewDragEnter(object sender, DragEventArgs e)
        {
            e.Handled = true;

            // Check that the data being dragged is a file
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Not allowed to drop.
                e.Effects = DragDropEffects.None;
            }

            // Get an array with the filenames of the files being dragged
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1 && (string.Compare(Path.GetExtension(files[0]), ".xml", true, CultureInfo.InvariantCulture) == 0))
                e.Effects = DragDropEffects.Move;
            else
                e.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Handles the Drop event of the settingsFilePath control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void SettingsFilePath_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;

            // Check that the data being dragged is a file
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            // Get an array with the filenames of the files being dragged
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1)
                SettingsFilePath = files[0];
        }

        /// <summary>
        /// Handles the Click event of the BrowseFileButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "XML Dateien (.xml)|*.xml";
            dialog.FileName = Path.GetFileName(SettingsFilePath);
            dialog.InitialDirectory = Path.GetDirectoryName(SettingsFilePath);

            if (dialog.ShowDialog() == true)
            {
                SettingsFilePath = dialog.FileName;
            }
        }

        /// <summary>
        /// Handles the CanExecute event of the OpenCommand control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CanExecuteRoutedEventArgs"/> instance containing the event data.</param>
        private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = File.Exists(SettingsFilePath);
        }

        /// <summary>
        /// Handles the Executed event of the OpenCommand control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            try
            {
                Settings = AppSettings.Load(SettingsFilePath);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Kann die angegebene Datei nicht laden: " + Environment.NewLine + ex,
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Click event of the SaveAndOpenDefault button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SaveAndOpenDefault_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var result = MessageBox.Show(
                        "Datei existiert bereits. Überschreiben?",
                        System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        MessageBoxResult.Yes);
                    if (result != MessageBoxResult.Yes)
                        return;
                }

                Settings = AppSettings.Default;
                Settings.Save(SettingsFilePath);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Kann die standard Einstellungen nicht auf den angegebenen Pfad speichern: " + Environment.NewLine + ex,
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Closes the application.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            Close();
        }
    }
}
