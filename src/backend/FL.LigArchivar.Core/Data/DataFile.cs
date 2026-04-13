using System.Collections.Immutable;
using System.IO.Abstractions;

namespace FL.LigArchivar.Core.Data;

public class DataFile
{
    private static readonly ImmutableList<string> KnownProperties = ImmutableList.Create(
        "_-6dB",
        "_-12dB",
        "_bearbeitet");

    private readonly IFileInfo _inner;

    public DataFile(IFileInfo file)
    {
        _inner = file;

        var name = file.FileSystem.Path.GetFileNameWithoutExtension(file.FullName);

        var property = string.Empty;
        foreach (var knownProperty in KnownProperties)
        {
            var propertyIndex = name.IndexOf(knownProperty, StringComparison.OrdinalIgnoreCase);
            if (propertyIndex > -1)
            {
                name = name[..propertyIndex];
                property = knownProperty;
                break;
            }
        }

        Name = name;
        Property = property;
        Extension = _inner.Extension;
    }

    public string Name { get; }
    public string Property { get; }
    public string Extension { get; }

    public string NameWithExtension => _inner.Name;
    public string FullName => _inner.FullName;
    public IDirectoryInfo Directory => _inner.Directory!;
    public DateTime LastWriteTimeUtc => _inner.LastWriteTimeUtc;

    public void MoveTo(string destFileName)
    {
        _inner.MoveTo(destFileName);
    }
}
