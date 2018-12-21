using System;
using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using FL.LigArchivar.Core.Utilities;

namespace FL.LigArchivar.Core.Data
{
    public class DataFile
    {
        private static readonly IImmutableList<string> _ignoredFiles = new string[]
        {
            ".BridgeCache",
            ".BridgeCacheT",
            "Thumbs.db",
        }.ToImmutableList();

        public DataFile(FileInfoBase file, EventDirectory parent)
        {
            var name = FileSystemProvider.Instance.Path.GetFileNameWithoutExtension(file.FullName);

            var regex = new Regex(Patterns.DataFile);

            var match = regex.Match(name);

            IsIgnored = _ignoredFiles.Any(item => item == file.Name);

            if (!match.Success)
                IsValid = false;
            else if (match.Groups.Count != 7)
                IsValid = false;
            else
                IsValid = true;

            Name = name;
            Parent = parent;

            Files = new FileInfoBase[] { file }.ToImmutableList();
        }

        public string Name { get; }

        public bool IsIgnored { get; }

        public bool IsValid { get; }

        public EventDirectory Parent { get; }

        public IImmutableList<FileInfoBase> Files { get; private set; }

        public void AddFile(DataFile file)
        {
            if (file.Name != Name)
                throw new InvalidOperationException($"Cannot add a file with name '{file.Name}' to the DataFile with name '{Name}'.");

            Files = Files.AddRange(file.Files);
        }
    }
}
