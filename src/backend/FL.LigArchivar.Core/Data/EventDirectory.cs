using System.Collections.Immutable;

using System.IO.Abstractions;

namespace FL.LigArchivar.Core.Data;

public class EventDirectory : IFileSystemItem
{
    private readonly IDirectoryInfo _directoryInfo;
    private readonly string _name;
    private readonly IFileSystemItem? _parent;
    private readonly bool _isValid;
    private readonly string _filePrefix;
    private readonly string _clubChar;
    private readonly string _year;
    private readonly string _month;
    private readonly string _day;
    private readonly string _eventName;
    
    public EventDirectory(IDirectoryInfo directoryInfo, string name, IFileSystemItem? parent, bool isValid, 
        string clubChar, string year, string month, string day, string eventName, string filePrefix)
    {
        _directoryInfo = directoryInfo;
        _name = name;
        _parent = parent;
        _isValid = isValid;
        _clubChar = clubChar;
        _year = year;
        _month = month;
        _day = day;
        _eventName = eventName;
        _filePrefix = filePrefix;
    }
    
    public string Name => _name;
    
    public bool IsValid => _isValid;
    
    public IFileSystemItem? Parent => _parent;
    
    public string ClubChar => _clubChar;
    
    public string Year => _year;
    
    public string Month => _month;
    
    public string Day => _day;
    
    public string EventName => _eventName;
    
    public string FilePrefix => _filePrefix;
    
    public IImmutableList<DataFiles> Files { get; set; } = ImmutableList<DataFiles>.Empty;
    
    public static bool TryCreate(IDirectoryInfo directoryInfo, IFileSystemItem? parent, out IFileSystemItem item)
    {
        // For simplicity, just return basic event directory
        item = new EventDirectory(directoryInfo, directoryInfo.Name, parent, true, "A", "2024", "05", "01", "EventName", "A_2024-05-01_");
        return true;
    }
}