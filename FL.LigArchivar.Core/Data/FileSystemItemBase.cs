using System;
using System.Collections.Immutable;
using System.IO;
using Caliburn.Micro;
using FL.LigArchivar.Core.Utilities;

namespace FL.LigArchivar.Core.Data
{
    public abstract class FileSystemItemBase : IFileSystemItem
    {
        public FileSystemItemBase(DirectoryInfo directory, string name, bool isValid, TryCreateFileSystemItem tryCreateChild = null)
        {
            Directory = directory;
            Name = name;
            IsValid = isValid;
            if (tryCreateChild != null)
            {
                Children = directory.GetChildrenFileSystemItems(tryCreateChild);
            }
            else
            {
                Children = ImmutableList<IFileSystemItem>.Empty;
            }
        }

        public DirectoryInfo Directory { get; }

        public string Name { get; }

        public bool IsValid { get; }

        public IImmutableList<IFileSystemItem> Children { get; }
    }
}
