using System.IO.Abstractions;

namespace FL.LigArchivar.Core
{
    internal static class FileSystemProvider
    {
        public static IFileSystem Instance { get; set; } = new FileSystem();
    }
}