using System.Collections.Immutable;

namespace FL.LigArchivar.Core.Data
{
    public class InvalidFileSystemItem : IFileSystemItem
    {
        public InvalidFileSystemItem(string name)
        {
            Name = "<invalid>" + name;
        }

        public string Name { get; }

        public bool Valid => false;

        public IImmutableList<IFileSystemItem> Children => ImmutableList<IFileSystemItem>.Empty;
    }
}
