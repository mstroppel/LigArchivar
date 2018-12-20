using System;
using System.IO;

namespace FL.LigArchivar.Core.Utilities
{
    public static class DirectoryEx
    {
        public static bool Exists(string path)
        {
            try
            {
                var attributes = FileSystemProvider.Instance.File.GetAttributes(path);
                return (attributes & FileAttributes.Directory) == FileAttributes.Directory;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (PathTooLongException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
