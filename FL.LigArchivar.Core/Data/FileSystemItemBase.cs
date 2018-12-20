using System.Collections.Immutable;

using System.IO.Abstractions;
using System.Linq;
using FL.LigArchivar.Core.Utilities;

namespace FL.LigArchivar.Core.Data
{
    public abstract class FileSystemItemBase : IFileSystemItem, IFileSystemItemWithChildren
    {
        private readonly bool _itemItselfIsValid;

        public FileSystemItemBase(DirectoryInfoBase directory, string name, IFileSystemItem parent, bool isValid, TryCreateFileSystemItem tryCreateChild = null)
        {
            Directory = directory;
            Name = name;
            Parent = parent;
            _itemItselfIsValid = isValid;

            if (tryCreateChild != null)
            {
                Children = directory.GetChildrenFileSystemItems(this, tryCreateChild);
            }
            else
            {
                Children = ImmutableList<IFileSystemItem>.Empty;
            }

            UpdateIsValid();
        }

        public DirectoryInfoBase Directory { get; }

        public string Name { get; }

        public bool IsValid { get; private set; }

        public IFileSystemItem Parent { get; }

        public IImmutableList<IFileSystemItem> Children { get; }

        private void UpdateIsValid()
        {
            if (!_itemItselfIsValid)
            {
                IsValid = false;
                return;
            }

            var allChildrenAreValid = Children.All(item => item.IsValid);
            IsValid = allChildrenAreValid;
        }
    }
}
