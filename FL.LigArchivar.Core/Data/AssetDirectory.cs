using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using FL.LigArchivar.Core.Utilities;

namespace FL.LigArchivar.Core.Data
{
    public class AssetDirectory : PropertyChangedBase, IFileSystemItem
    {
        private static readonly IImmutableList<string> _allowedNames = new[]
        {
            @"Digitalfoto",
            @"Ton",
            @"Video"
        }.ToImmutableList();
        private readonly DirectoryInfo _assetDirectory;

        private AssetDirectory(DirectoryInfo assetDirectory)
        {
            _assetDirectory = assetDirectory;
            Name = assetDirectory.Name;
            Children = GetChildren(assetDirectory);
        }

        public static bool TryCreate(DirectoryInfo assetDirectory, out AssetDirectory directory)
        {
            directory = null;

            if (!DirectoryEx.Exists(assetDirectory.FullName))
                return false;

            var name = assetDirectory.Name;

            if (!_allowedNames.Any(item => item == name))
                return false;

            directory = new AssetDirectory(assetDirectory);
            return true;
        }

        public string Name { get; }

        public bool HasChildren { get; }

        public IImmutableList<IFileSystemItem> Children { get; }

        public bool Valid => true;

        private static IImmutableList<IFileSystemItem> GetChildren(DirectoryInfo directory)
        {
            var items = new List<IFileSystemItem>();

            var subDirectories = directory.GetDirectories();
            foreach (var subDirectory in subDirectories)
            {
                var isYearDirectory = YearDirectory.TryCreate(subDirectory, out var year);
                if (isYearDirectory)
                {
                    items.Add(year);
                }
                else
                {
                    var invalidItem = new InvalidFileSystemItem(subDirectory.Name);
                    items.Add(invalidItem);
                }
            }

            return items.ToImmutableList();
        }
    }
}
