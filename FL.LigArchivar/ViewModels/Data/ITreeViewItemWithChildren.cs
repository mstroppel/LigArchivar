using System.Collections.Immutable;

namespace FL.LigArchivar.ViewModels.Data
{
    public interface ITreeViewItemWithChildren
    {
        IImmutableList<ITreeViewItem> Children { get; }
    }
}
