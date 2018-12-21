using System.IO;
using System.Linq;
using FL.LigArchivar.Core.Data;

namespace FL.LigArchivar.ViewModels.Data
{
    public class FileListItem
    {
        private readonly DataFile _inner;

        public FileListItem(DataFile inner)
        {
            _inner = inner;

            var extensions = inner.Files
                .Select(item => Path.GetExtension(item.FullName));

            var extensionsAsString = string.Join(", ", extensions);
            Extensions = extensionsAsString;
        }

        public string Name => _inner.Name;

        public string Extensions { get; }

        public bool IsValid => _inner.IsValid;
    }
}
