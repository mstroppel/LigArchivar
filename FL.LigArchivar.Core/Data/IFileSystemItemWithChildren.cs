using System.Collections.Immutable;

namespace FL.LigArchivar.Core.Data
{
    public interface IFileSystemItemWithChildren
    {
        IImmutableList<IFileSystemItem> Children { get; }
    }
}
