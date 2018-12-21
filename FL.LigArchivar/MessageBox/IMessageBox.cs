namespace FL.LigArchivar.MessageBox
{
    public interface IMessageBox
    {
        /// <summary>
        /// Shows an exception.
        /// </summary>
        /// <param name="e">The exception.</param>
        void ShowException(System.Exception e);
    }
}
