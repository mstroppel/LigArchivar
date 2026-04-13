using System.Collections.Immutable;
using System.IO.Abstractions;
using FL.LigArchivar.Core.Utilities;

namespace FL.LigArchivar.Core.Data;

public class AssetDirectory : FileSystemItemBase
{
    private const string FolderStructureFolderName = "Ordnerstruktur";

    private static readonly ImmutableList<string> AllowedNames = ImmutableList.Create(
        "Digitalfoto",
        "Ton",
        "Video");

    private AssetDirectory(IDirectoryInfo assetDirectory, IFileSystemItem? parent)
        : base(assetDirectory, assetDirectory.Name, parent, true, TryCreateChild)
    {
        IsPicturesDirectory = assetDirectory.Name == "Digitalfoto";
    }

    public bool IsPicturesDirectory { get; }

    public static bool TryCreate(IDirectoryInfo assetDirectory, IFileSystemItem? parent, out IFileSystemItem directory)
    {
        directory = null!;

        if (!DirectoryEx.Exists(assetDirectory.FileSystem, assetDirectory.FullName))
            return false;

        var name = assetDirectory.Name;

        if (AllowedNames.All(item => item != name))
            return false;

        directory = new AssetDirectory(assetDirectory, parent);
        return true;
    }

    private static bool TryCreateChild(IDirectoryInfo directory, IFileSystemItem? parent, out IFileSystemItem fileSystemItem)
    {
        if (YearDirectory.TryCreate(directory, parent, out fileSystemItem))
            return true;

        if (directory.Name == FolderStructureFolderName)
        {
            fileSystemItem = new IgnoredFileSystemItem(directory, parent);
            return true;
        }

        fileSystemItem = null!;
        return false;
    }
}
