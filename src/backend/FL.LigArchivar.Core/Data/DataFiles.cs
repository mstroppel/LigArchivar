using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using FL.LigArchivar.Core.Utilities;

namespace FL.LigArchivar.Core.Data;

public class DataFiles
{
    private const string LonelyExtension = ".dng";

    private static readonly ImmutableList<string> IgnoredFiles = ImmutableList.Create("Thumbs.db");

    private readonly ILogger _logger;
    private IImmutableList<DataFile> _files;

    public DataFiles(DataFile dataFile, EventDirectory parent, ILogger logger)
    {
        _logger = logger;
        Name = dataFile.Name;
        IsIgnored = IgnoredFiles.Any(item => item == dataFile.NameWithExtension);
        IsValid = GetIsValid(dataFile.Name, parent);
        Parent = parent;
        _files = ImmutableList.Create(dataFile);
        UpdateIsLonely();
    }

    public string Name { get; }

    public bool IsIgnored { get; }

    public bool IsValid { get; }

    public bool IsLonely { get; private set; }

    public EventDirectory Parent { get; }

    public IImmutableList<DataFile> Files => _files;

    public void AddFile(DataFile file)
    {
        if (file.Name != Name)
            throw new InvalidOperationException(
                $"Cannot add a file with name '{file.Name}' to the DataFiles with name '{Name}'.");

        _files = _files.Add(file);
        UpdateIsLonely();
    }

    internal void Delete()
    {
        foreach (var file in _files)
        {
            _logger.LogInformation("Deleting file '{FullName}'.", file.FullName);
            file.Directory.FileSystem.File.Delete(file.FullName);
        }
    }

    public void RenameFiles(string newNameWithoutExtension)
    {
        foreach (var file in _files)
        {
            var directory = file.Directory.FullName;
            var fileName = newNameWithoutExtension + file.Property + file.Extension;
            var newPath = file.Directory.FileSystem.Path.Combine(directory, fileName);

            if (newPath == file.FullName)
                continue;

            if (file.Directory.FileSystem.File.Exists(newPath))
            {
                var message =
                    "Kann die Datei nicht umbenennen, da eine Datei mit dem Zielnamen schon existiert. " +
                    "Wurde im Ordner schon einmal umbenannt? Bitte händisch korrigieren." +
                    Environment.NewLine +
                    $"  Quelldatei: {file.Name}" + Environment.NewLine +
                    $"  Zieldatei: {fileName} (existiert bereits)";
                throw new RenameException(message);
            }

            _logger.LogInformation("Moving '{Source}' to '{Dest}'.", file.FullName, newPath);
            file.MoveTo(newPath);
        }
    }

    internal void RenameFilesToFileDateTime(HashSet<string> usedBaseNames)
    {
        var fileForDateTime = GetFileForDateTime();
        var rawBaseName = fileForDateTime.LastWriteTimeUtc.ToString(
            "yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);

        // Resolve collisions: if rawBaseName is already taken (by a previously
        // renamed group in this batch, or by an existing file on disk), append
        // a counter suffix until we find a free name.
        var newBaseName = rawBaseName;
        var counter = 2;
        while (!usedBaseNames.Add(newBaseName))
        {
            newBaseName = $"{rawBaseName}_{counter}";
            counter++;
        }

        foreach (var file in _files)
        {
            var directory = file.Directory.FullName;
            var fileName = newBaseName + file.Property + file.Extension;
            var newPath = file.Directory.FileSystem.Path.Combine(directory, fileName);

            if (newPath == file.FullName)
                continue;

            if (file.Directory.FileSystem.File.Exists(newPath))
            {
                var message =
                    "Kann die Datei nicht umbenennen, da eine Datei mit dem Zielnamen schon existiert. " +
                    "Wurde im Ordner schon einmal umbenannt? Bitte händisch korrigieren." +
                    Environment.NewLine +
                    $"  Quelldatei: {file.Name}" + Environment.NewLine +
                    $"  Zieldatei: {fileName} (existiert bereits)";
                throw new RenameException(message);
            }

            _logger.LogInformation("Moving '{Source}' to '{Dest}'.", file.FullName, newPath);
            file.MoveTo(newPath);
        }
    }

    private static bool GetIsValid(string name, EventDirectory parent)
    {
        var regex = new Regex(Patterns.DataFile);
        var match = regex.Match(name);

        if (!match.Success || match.Groups.Count != 8)
            return false;

        var clubChar = match.Groups[1].Value;
        if (clubChar != parent.ClubChar) return false;

        var year = match.Groups[2].Value;
        if (year != parent.Year) return false;

        var month = match.Groups[3].Value;
        if (month != parent.Month) return false;

        var day = match.Groups[4].Value;
        if (day != parent.Day) return false;

        return true;
    }

    private DataFile GetFileForDateTime()
    {
        return _files.FirstOrDefault(f => f.Extension == ".jpg")
            ?? _files.FirstOrDefault(f => f.Extension == ".mp3")
            ?? _files.FirstOrDefault(f => f.Extension == ".mts")
            ?? _files[0];
    }

    private void UpdateIsLonely()
    {
        IsLonely = _files.Count == 1
            && string.Compare(LonelyExtension, _files[0].Extension, StringComparison.OrdinalIgnoreCase) == 0;
    }
}
