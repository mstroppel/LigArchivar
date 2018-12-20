using System.Collections.Immutable;
using System.IO;
using System.Linq;
using FL.LigArchivar.Core.Utilities;

namespace FL.LigArchivar.Core.Data
{
    public abstract class FileSystemItemBase : IFileSystemItem
    {
        private readonly bool _itemItselfIsValid;

        public FileSystemItemBase(DirectoryInfo directory, string name, bool isValid, TryCreateFileSystemItem tryCreateChild = null)
        {
            Directory = directory;
            Name = name;
            _itemItselfIsValid = isValid;

            if (tryCreateChild != null)
            {
                Children = directory.GetChildrenFileSystemItems(tryCreateChild);
            }
            else
            {
                Children = ImmutableList<IFileSystemItem>.Empty;
            }

            UpdateIsValid();
        }

        public DirectoryInfo Directory { get; }

        public string Name { get; }

        public bool IsValid { get; private set;  }

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
