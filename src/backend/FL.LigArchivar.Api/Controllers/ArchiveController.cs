using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FL.LigArchivar.Api.Models;
using FL.LigArchivar.Api.Services;

namespace FL.LigArchivar.Api.Controllers;

[ApiController]
[Route("api/archive")]
[Authorize]
public class ArchiveController : ControllerBase
{
    private readonly ArchiveService _archiveService;

    public ArchiveController(ArchiveService archiveService)
    {
        _archiveService = archiveService;
    }

    /// <summary>
    /// Returns the full archive tree or a subtree starting from the given path.
    /// </summary>
    [HttpGet("tree")]
    public ActionResult<TreeNodeDto[]> GetTree([FromQuery] string? path = null)
    {
        if (path != null && !ArchiveService.ValidatePath(path))
            return BadRequest("Invalid path.");

        var tree = _archiveService.GetTree(path);
        return Ok(tree);
    }
}
