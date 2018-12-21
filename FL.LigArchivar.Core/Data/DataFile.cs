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
            Name = name;
            IsIgnored = _ignoredFiles.Any(item => item == file.Name);
            IsValid = GetIsValid(name, parent);
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

        private static bool GetIsValid(string name, EventDirectory parent)
        {
            var regex = new Regex(Patterns.DataFile);
            var match = regex.Match(name);

            if (!match.Success)
                return false;

            if (match.Groups.Count != 8)
                return false;

            var clubChar = match.Groups[1].Value;
            if (clubChar != parent.ClubChar)
                return false;

            var year = match.Groups[2].Value;
            if (year != parent.Year)
                return false;

            var month = match.Groups[3].Value;
            if (month != parent.Month)
                return false;

            var day = match.Groups[4].Value;
            if (day != parent.Day)
                return false;

            return true;
        }
    }
}
