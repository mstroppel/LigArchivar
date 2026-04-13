using System.Globalization;

namespace FL.LigArchivar.Core.Data;

public static class FileSystemItemExtensions
{
    public static string? GetYear(this IFileSystemItem? self)
    {
        if (self == null)
            return null;

        if (self is YearDirectory selfAsYear)
            return selfAsYear.Name;

        return GetYear(self.Parent);
    }

    public static string? GetClubChar(this IFileSystemItem? self)
    {
        if (self == null)
            return null;

        if (self is ClubDirectory selfAsClub)
            return selfAsClub.Name[0].ToString(CultureInfo.InvariantCulture);

        return GetClubChar(self.Parent);
    }

    public static IFileSystemItem? GetChild(this IFileSystemItemWithChildren self, string path)
    {
        var parts = path.Split('/', '\\')
            .Where(p => !string.IsNullOrWhiteSpace(p));

        return self.GetChild(parts);
    }

    public static IFileSystemItem? GetChild(this IFileSystemItemWithChildren self, IEnumerable<string> path)
    {
        var name = path.FirstOrDefault();
        if (name == null)
            return self as IFileSystemItem;

        var child = self.Children.FirstOrDefault(item => item.Name == name);
        if (child == null)
            return null;

        var nextPaths = path.Skip(1).ToList();
        if (nextPaths.Count == 0)
            return child;

        if (child is IFileSystemItemWithChildren childWithChildren)
            return childWithChildren.GetChild(nextPaths);

        return child;
    }

    public static bool IsInPictures(this IFileSystemItem self)
    {
        if (self is AssetDirectory asAsset)
            return asAsset.IsPicturesDirectory;

        if (self.Parent == null)
            return false;

        return self.Parent.IsInPictures();
    }
}
