using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FL.LigArchivar.Api.Models;
using FL.LigArchivar.Api.Services;

namespace FL.LigArchivar.Api.Controllers;

[ApiController]
[Route("api/events")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly ArchiveService _archiveService;

    public EventsController(ArchiveService archiveService)
    {
        _archiveService = archiveService;
    }

    /// <summary>
    /// Returns event details including file list.
    /// GET /api/events/{**path}
    /// </summary>
    [HttpGet("{**path}")]
    public ActionResult<EventDetailDto> GetEvent(string path)
    {
        if (!ArchiveService.ValidatePath(path))
            return BadRequest("Invalid path.");

        var dto = _archiveService.GetEvent(path);
        if (dto == null)
            return NotFound();

        return Ok(dto);
    }

    /// <summary>
    /// Sequential rename. Acquires write lock; returns 409 if locked.
    /// POST /api/events/rename?path=Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung
    /// </summary>
    [HttpPost("rename")]
    public ActionResult<EventDetailDto> Rename([FromQuery] string path, [FromBody] RenameRequestDto request)
    {
        if (!ArchiveService.ValidatePath(path))
            return BadRequest("Invalid path.");

        var (result, error) = _archiveService.Rename(path, request);

        return error switch
        {
            null => Ok(result),
            "locked" => Conflict(new { message = "A rename operation is already in progress." }),
            "not-found" => NotFound(),
            "invalid-path" => BadRequest("Invalid path."),
            _ => UnprocessableEntity(new { message = error })
        };
    }

    /// <summary>
    /// DateTime-based rename. Acquires write lock; returns 409 if locked.
    /// POST /api/events/rename-by-datetime?path=Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung
    /// </summary>
    [HttpPost("rename-by-datetime")]
    public ActionResult<EventDetailDto> RenameByDateTime([FromQuery] string path)
    {
        if (!ArchiveService.ValidatePath(path))
            return BadRequest("Invalid path.");

        var (result, error) = _archiveService.RenameByDateTime(path);

        return error switch
        {
            null => Ok(result),
            "locked" => Conflict(new { message = "A rename operation is already in progress." }),
            "not-found" => NotFound(),
            "invalid-path" => BadRequest("Invalid path."),
            _ => UnprocessableEntity(new { message = error })
        };
    }
}
