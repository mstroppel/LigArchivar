using System;

namespace FL.LigArchivar.MessageBox
{
    /// <summary>
    /// Implementation of <see cref="IMessageBox"/> for a XAML message box.
    /// </summary>
    public class XamlMessageBox : IMessageBox
    {
        /// <summary>
        /// Shows an exception.
        /// </summary>
        /// <param name="e">The exception.</param>
        public void ShowException(System.Exception e)
        {
            System.Windows.MessageBox.Show(
                "Fehler: " + e.Message + Environment.NewLine + "Stacktrace: " + e.StackTrace,
                "Fehler",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error,
                System.Windows.MessageBoxResult.OK);
        }
    }
}
