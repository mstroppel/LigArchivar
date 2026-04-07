using System.IO.Abstractions;
using FL.LigArchivar.Api.Models;
using FL.LigArchivar.Core;
using FL.LigArchivar.Core.Data;

namespace FL.LigArchivar.Api.Services;

/// <summary>
/// Thin service that bridges the API layer with the Core domain model.
/// Caches the archive tree and serializes write operations.
/// </summary>
public sealed class ArchiveService : IDisposable
{
    private const string ArchiveRoot = "/archive";

    private readonly IFileSystem _fileSystem;
    private readonly ILogger<ArchiveService> _logger;

    // Write operations are serialized; if already locked, 409 is returned.
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    // Simple in-memory cache: reload tree on every read request (archive is local I/O).
    // For large archives, consider caching with a stale-on-write invalidation strategy.

    public ArchiveService(IFileSystem fileSystem, ILogger<ArchiveService> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    // ── Tree ──────────────────────────────────────────────────────────────────

    public TreeNodeDto[] GetTree(string? relativePath = null)
    {
        if (!Core.ArchiveRoot.TryCreate(ArchiveRoot, _fileSystem, out var root) || root == null)
            return [];

        IEnumerable<IFileSystemItem> items = root.Children;

        if (!string.IsNullOrWhiteSpace(relativePath))
        {
            var node = root.GetChild(relativePath);
            if (node is IFileSystemItemWithChildren container)
                items = container.Children;
            else
                return [];
        }

        return items.Select(item => MapToTreeNode(item, relativePath)).ToArray();
    }

    // ── Event ─────────────────────────────────────────────────────────────────

    public EventDetailDto? GetEvent(string path)
    {
        if (!ValidatePath(path)) return null;

        if (!Core.ArchiveRoot.TryCreate(ArchiveRoot, _fileSystem, out var root) || root == null)
            return null;

        var node = root.GetChild(path);
        if (node is not EventDirectory eventDir)
            return null;

        eventDir.LoadChildren();
        return MapToEventDetail(eventDir, path);
    }

    // ── Rename ────────────────────────────────────────────────────────────────

    /// <returns>
    ///   (dto, null) on success,
    ///   (null, "locked") if a rename is already in progress,
    ///   (null, message) on RenameException.
    /// </returns>
    public (EventDetailDto? Result, string? Error) Rename(string path, RenameRequestDto request)
    {
        if (!ValidatePath(path)) return (null, "invalid-path");

        if (!_writeLock.Wait(0))
            return (null, "locked");

        try
        {
            if (!Core.ArchiveRoot.TryCreate(ArchiveRoot, _fileSystem, out var root) || root == null)
                return (null, "archive-not-found");

            var node = root.GetChild(path);
            if (node is not EventDirectory eventDir)
                return (null, "not-found");

            eventDir.LoadChildren();
            eventDir.Rename(request.StartNumber, request.FileOrder);

            return (MapToEventDetail(eventDir, path), null);
        }
        catch (RenameException ex)
        {
            _logger.LogWarning("Rename failed for '{Path}': {Message}", path, ex.Message);
            return (null, ex.Message);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public (EventDetailDto? Result, string? Error) RenameByDateTime(string path)
    {
        if (!ValidatePath(path)) return (null, "invalid-path");

        if (!_writeLock.Wait(0))
            return (null, "locked");

        try
        {
            if (!Core.ArchiveRoot.TryCreate(ArchiveRoot, _fileSystem, out var root) || root == null)
                return (null, "archive-not-found");

            var node = root.GetChild(path);
            if (node is not EventDirectory eventDir)
                return (null, "not-found");

            eventDir.LoadChildren();
            eventDir.RenameToFileDateTime();

            return (MapToEventDetail(eventDir, path), null);
        }
        catch (RenameException ex)
        {
            _logger.LogWarning("RenameByDateTime failed for '{Path}': {Message}", path, ex.Message);
            return (null, ex.Message);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    // ── Mapping ───────────────────────────────────────────────────────────────

    private static TreeNodeDto MapToTreeNode(IFileSystemItem item, string? parentPath)
    {
        var nodePath = string.IsNullOrEmpty(parentPath)
            ? item.Name
            : parentPath.TrimEnd('/') + "/" + item.Name;

        var nodeType = item switch
        {
            AssetDirectory => "asset",
            YearDirectory => "year",
            ClubDirectory => "club",
            EventDirectory => "event",
            IgnoredFileSystemItem => "ignored",
            InvalidFileSystemItem => "invalid",
            _ => "unknown"
        };

        TreeNodeDto[]? children = null;
        if (item is IFileSystemItemWithChildren container && item is not EventDirectory)
            children = container.Children.Select(c => MapToTreeNode(c, nodePath)).ToArray();

        return new TreeNodeDto(item.Name, nodePath, item.IsValid, nodeType, children);
    }

    private static EventDetailDto MapToEventDetail(EventDirectory eventDir, string path)
    {
        var files = eventDir.Children
            .Where(f => !f.IsIgnored)
            .Select(f => new FileGroupDto(
                f.Name,
                f.Files.Select(fi => fi.Extension).ToArray(),
                f.Files.Select(fi => fi.Property).Where(p => !string.IsNullOrEmpty(p)).ToArray(),
                f.IsValid,
                f.IsLonely,
                f.Files.Count > 0 ? f.Files[0].LastWriteTimeUtc : DateTime.MinValue
            ))
            .ToArray();

        return new EventDetailDto(
            eventDir.Name,
            path,
            eventDir.FilePrefix,
            eventDir.IsValid,
            eventDir.IsInPictures(),
            files);
    }

    // ── Path validation ───────────────────────────────────────────────────────

    public static bool ValidatePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;

        // Prevent directory traversal
        if (path.Contains("..")) return false;
        if (path.Contains('\0')) return false;

        // Must not be rooted
        if (System.IO.Path.IsPathRooted(path)) return false;

        return true;
    }

    public void Dispose()
    {
        _writeLock.Dispose();
    }
}
