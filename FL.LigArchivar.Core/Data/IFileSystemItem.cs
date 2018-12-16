using System.Collections.Immutable;

namespace FL.LigArchivar.Core.Data
{
    public interface IFileSystemItem
    {
        string Name { get; }

        bool Valid { get; }

        IImmutableList<IFileSystemItem> Children { get; }
    }
}
