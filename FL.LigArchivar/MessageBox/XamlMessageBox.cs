using System;

namespace FL.LigArchivar.MessageBox
{
    /// <summary>
    /// Implementation of <see cref="IMessageBox"/> for a XAML message box.
    /// </summary>
    public class XamlMessageBox : IMessageBox
    {
        public void ShowException(Exception e)
        {
            System.Windows.MessageBox.Show(
                $"Fehler: {e.Message} ({e.GetType().FullName})" + Environment.NewLine +
                $"Stacktrace: {e.StackTrace}",
                "Fehler",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error,
                System.Windows.MessageBoxResult.OK);
        }

        public void ShowErrorMessage(string title, string message)
        {
            System.Windows.MessageBox.Show(
                message,
                title,
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error,
                System.Windows.MessageBoxResult.OK);
        }
    }
}
