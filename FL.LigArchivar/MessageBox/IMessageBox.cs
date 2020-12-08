namespace FL.LigArchivar.MessageBox
{
    public interface IMessageBox
    {
        /// <summary>
        /// Shows an exception.
        /// </summary>
        /// <param name="e">The exception.</param>
        void ShowException(System.Exception e);

        /// <summary>
        /// Show a message to the user.
        /// </summary>
        /// <param name="title">The title of the message box.</param>
        /// <param name="message">The message to show.</param>
        void ShowErrorMessage(string title, string message);
    }
}
