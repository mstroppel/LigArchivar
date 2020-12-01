using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using Caliburn.Micro;
using FL.LigArchivar.Core.Utilities;

namespace FL.LigArchivar.Core.Data
{
    public class DataFiles
    {
        private const string LonelyExtension = ".dng";
        private static readonly IImmutableList<string> _ignoredFiles = new string[]
        {
            "Thumbs.db",
        }.ToImmutableList();

        private static readonly ILog _log = LogManager.GetLog(typeof(EventDirectory));

        public DataFiles(DataFile dataFile, EventDirectory parent)
        {
            Name = dataFile.Name;
            IsIgnored = _ignoredFiles.Any(item => item == dataFile.NameWithExtension);
            IsValid = GetIsValid(dataFile.Name, parent);
            Parent = parent;

            Files = new DataFile[] { dataFile }.ToImmutableList();
        }

        public string Name { get; }

        public bool IsIgnored { get; }

        public bool IsValid { get; }

        public bool IsLonely { get; private set; }

        public EventDirectory Parent { get; }

        public IImmutableList<DataFile> Files
        {
            get => _files;
            private set
            {
                if (_files != value)
                {
                    _files = value;

                    if (_files.Count == 1)
                    {
                        var isLonely = string.Compare(LonelyExtension, _files[0].Extension, true, CultureInfo.InvariantCulture) == 0;
                        IsLonely = isLonely;
                    }
                    else
                    {
                        IsLonely = false;
                    }
                }
            }
        }

        private IImmutableList<DataFile> _files;

        public void AddFile(DataFile file)
        {
            if (file.Name != Name)
                throw new InvalidOperationException($"Cannot add a file with name '{file.Name}' to the DataFiles with name '{Name}'.");

            Files = Files.Add(file);
        }

        internal void Delete()
        {
            foreach (var file in Files)
            {
                _log.Info($"Deleting file '{file.FullName}'.");
                FileSystemProvider.Instance.File.Delete(file.FullName);
            }
        }

        public void RenameFiles(string newNameWithoutExtension)
        {
            foreach (var file in Files)
            {
                var directory = file.Directory.FullName;
                var fileName = newNameWithoutExtension + file.Property + file.Extension;

                var newPath = FileSystemProvider.Instance.Path.Combine(directory, fileName);

                if (newPath == file.FullName)
                    continue;

                _log.Info($"Moving '{file.FullName}' to '{newPath}'.");
                file.MoveTo(newPath);
            }
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
