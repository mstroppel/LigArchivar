namespace FL.LigArchivar.Core;

/// <summary>
/// A well-known exception whose message should be shown to the user.
/// </summary>
public class RenameException : Exception
{
    public RenameException() { }

    public RenameException(string message)
        : base(message) { }

    public RenameException(string message, Exception? innerException)
        : base(message, innerException) { }
}
