using System;
using System.IO;

namespace FL.LigArchivar.Core.Utilities
{
    /// <summary>
    /// Helper methods related to directories.
    /// </summary>
    public static class DirectoryEx
    {
        /// <summary>
        /// Checks if the given path is a directory and exists.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns><c>true</c> if the given path is a directory and exists; otherwise <c>false</c>.</returns>
        public static bool Exists(string path)
        {
            try
            {
                var attributes = File.GetAttributes(path);
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
