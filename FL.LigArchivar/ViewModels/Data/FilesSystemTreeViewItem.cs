using System.Collections.Immutable;
using System.Linq;
using FL.LigArchivar.Core.Data;

namespace FL.LigArchivar.ViewModels.Data
{
    internal class FilesSystemTreeViewItem : TreeViewItemBase
    {
        private readonly IFileSystemItem _inner;

        public FilesSystemTreeViewItem(IFileSystemItem inner, ITreeViewItem parent = null)
            : base(inner.Name, parent, inner.IsValid)
        {
            _inner = inner;

            var asWithChildren = inner as IFileSystemItemWithChildren;
            if (asWithChildren != null)
            {
                Children = asWithChildren.Children
                    .Where(item => !(item is IgnoredFileSystemItem))
                    .Select(item => item.ToTreeViewItem(this))
                    .ToImmutableList();
            }
            else
            {
                Children = ImmutableList<ITreeViewItem>.Empty;
            }
        }

        public override IImmutableList<ITreeViewItem> Children { get; }
    }
}
