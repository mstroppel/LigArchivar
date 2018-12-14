using System.Collections.Immutable;

namespace FL.LigArchivar.ViewModels.Data
{
    public interface ITreeViewItem
    {
        string Name { get; }

        string FullPath { get; }

        IImmutableList<ITreeViewItem> Children { get; }
    }
}
