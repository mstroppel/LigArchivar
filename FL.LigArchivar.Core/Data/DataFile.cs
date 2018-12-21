using System;
using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using FL.LigArchivar.Core.Utilities;

namespace FL.LigArchivar.Core.Data
{
    public class DataFile
    {
        public DataFile(FileInfoBase file, EventDirectory parent)
        {
            var name = FileSystemProvider.Instance.Path.GetFileNameWithoutExtension(file.FullName);

            var regex = new Regex(Patterns.DataFile);

            var match = regex.Match(name);

            if (!match.Success)
                IsValid = false;
            else if (match.Groups.Count != 6)
                IsValid = false;
            else
                IsValid = true;

            Name = name;
            Parent = parent;

            Files = new FileInfoBase[] { file }.ToImmutableList();
        }

        public string Name { get; }

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
