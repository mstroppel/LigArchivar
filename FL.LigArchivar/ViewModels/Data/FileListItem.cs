using System.Linq;
using FL.LigArchivar.Core.Data;

namespace FL.LigArchivar.ViewModels.Data
{
    public class FileListItem
    {
        private readonly DataFiles _inner;

        public FileListItem(DataFiles inner)
        {
            _inner = inner;

            var extensions = inner.Files.Select(item => item.Extension).Distinct();
            var extensionsAsString = string.Join(", ", extensions);
            Extensions = extensionsAsString;

            var properties = inner.Files.Select(item => item.Property).Select(item => string.IsNullOrWhiteSpace(item) ? "''" : item);
            var propertiesAsString = string.Join(", ", properties);
            Properties = propertiesAsString;
        }

        public string Name => _inner.Name;

        public string Extensions { get; }

        public string Properties { get; }

        public bool IsValid => _inner.IsValid;

        public bool IsLonely => _inner.IsLonely;
    }
}
