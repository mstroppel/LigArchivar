using System;

namespace FL.LigArchivar.Core
{
    /// <summary>
    /// This is a well known exception. The message has to e shown to the user.
    /// </summary>
    public class RenameException : Exception
    {
        public RenameException()
        {
        }

        public RenameException(string message)
            : base(message)
        {
        }

        public RenameException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
