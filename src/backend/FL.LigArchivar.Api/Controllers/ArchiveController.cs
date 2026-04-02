using Microsoft.AspNetCore.Mvc;

namespace FL.LigArchivar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArchiveController : ControllerBase
{
    // Simple endpoint to get archive tree structure
    [HttpGet("tree")]
    public IActionResult GetTree()
    {
        // Create a minimal mock response to demonstrate functionality
        var tree = new[]
        {
            new
            {
                Name = "Digitalfoto",
                Path = "Digitalfoto",
                IsValid = true,
                NodeType = "asset",
                Children = new[]
                {
                    new
                    {
                        Name = "2024",
                        Path = "Digitalfoto/2024",
                        IsValid = true,
                        NodeType = "year",
                        Children = new[]
                        {
                            new
                            {
                                Name = "A-Albverein",
                                Path = "Digitalfoto/2024/A-Albverein",
                                IsValid = true,
                                NodeType = "club",
                                Children = new[]
                                {
                                    new
                                    {
                                        Name = "A_2024-05-01_Maiwanderung",
                                        Path = "Digitalfoto/2024/A-Albverein/A_2024-05-01_Maiwanderung",
                                        IsValid = true,
                                        NodeType = "event",
                                        Children = null
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        return Ok(tree);
    }

    // Endpoint to get event details
    [HttpGet("events/{path}")]
    public IActionResult GetEvent(string path)
    {
        var eventDetail = new
        {
            Name = "A_2024-05-01_Maiwanderung",
            Path = path,
            FilePrefix = "A_2024-05-01_",
            IsValid = true,
            IsInPictures = true,
            Files = new[]
            {
                new
                {
                    Name = "A_2024-05-01_001",
                    Extensions = new[] { ".jpg" },
                    Properties = new[] { "" },
                    IsValid = true,
                    IsLonely = false
                }
            }
        };

        return Ok(eventDetail);
    }
}