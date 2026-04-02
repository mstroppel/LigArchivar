using System.Collections.Immutable;

namespace FL.LigArchivar.Core.Data;

public interface IFileSystemItemWithChildren : IFileSystemItem
{
    IImmutableList<IFileSystemItem> Children { get; }
}