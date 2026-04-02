using System.Collections.Immutable;

using System.IO.Abstractions;

namespace FL.LigArchivar.Core.Data;

public abstract class FileSystemItemBase : IFileSystemItemWithChildren
{
    private readonly IDirectoryInfo _directoryInfo;
    private readonly string _name;
    private readonly IFileSystemItem? _parent;
    private readonly bool _isValid;
    private readonly IImmutableList<IFileSystemItem> _children;
    
    protected FileSystemItemBase(IDirectoryInfo directoryInfo, string name, IFileSystemItem? parent, bool isValid, Func<IDirectoryInfo, IFileSystemItem, out IFileSystemItem>? tryCreateChild)
    {
        _directoryInfo = directoryInfo;
        _name = name;
        _parent = parent;
        _isValid = isValid;
        
        if (tryCreateChild != null)
        {
            _children = directoryInfo.GetChildrenFileSystemItems(parent, tryCreateChild)
                .ToImmutableList();
        }
        else
        {
            _children = ImmutableList<IFileSystemItem>.Empty;
        }
    }
    
    public string Name => _name;
    
    public bool IsValid => _isValid;
    
    public IFileSystemItem? Parent => _parent;
    
    public IImmutableList<IFileSystemItem> Children => _children;
    
    public IDirectoryInfo Directory => _directoryInfo;
}