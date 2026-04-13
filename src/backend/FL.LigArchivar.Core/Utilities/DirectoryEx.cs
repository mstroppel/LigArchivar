using System.IO.Abstractions;

namespace FL.LigArchivar.Core.Utilities;

public static class DirectoryEx
{
    public static bool Exists(IFileSystem fileSystem, string path)
    {
        try
        {
            var attributes = fileSystem.File.GetAttributes(path);
            return (attributes & FileAttributes.Directory) == FileAttributes.Directory;
        }
        catch (ArgumentException) { return false; }
        catch (PathTooLongException) { return false; }
        catch (NotSupportedException) { return false; }
        catch (FileNotFoundException) { return false; }
        catch (DirectoryNotFoundException) { return false; }
        catch (IOException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
    }
}
