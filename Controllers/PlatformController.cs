using Microsoft.AspNetCore.Mvc;
using tree_form_API.Models;
using tree_form_API.Services;

[ApiController]
[Route("api/[controller]")]
public class PlatformController : ControllerBase
{
    private readonly PlatformService _platformService;
    private readonly ILogger<UsersController> _logger;

    public PlatformController(PlatformService platformService, ILogger<UsersController> logger)
    {
        _platformService = platformService;
        _logger = logger;
    }

    // GET: api/Platform
    [HttpGet]
    public async Task<IActionResult> GetAllPlatforms()
    {
        _logger.LogInformation("GetAllPlatforms: Fetching all platforms.");
        var platforms = await _platformService.GetAllPlatformsAsync();
        _logger.LogInformation("GetAllPlatforms: Successfully fetched {Count} platforms.", platforms.Count);
        return Ok(platforms);
    }

    // GET: api/Platform/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPlatformById(Guid id)
    {
        _logger.LogInformation("GetPlatformById: Fetching platform with ID {PlatformId}.", id);

        var platform = await _platformService.GetPlatformByIdAsync(id);
        if (platform == null)
        {
            _logger.LogWarning("GetPlatformById: Platform with ID {PlatformId} not found.", id);
            return NotFound($"Platform with ID {id} not found.");
        }

        _logger.LogInformation("GetPlatformById: Successfully retrieved platform with ID {PlatformId}.", id);
        return Ok(platform);
    }

    // POST: api/Platform
    [HttpPost]
    public async Task<IActionResult> AddPlatform([FromBody] Platform platform)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("AddPlatform: Invalid platform data received.");
            return BadRequest(ModelState);
        }

        _logger.LogInformation("AddPlatform: Adding a new platform.");

        try
        {
            await _platformService.AddPlatformAsync(platform);
            _logger.LogInformation("AddPlatform: Platform with ID {PlatformId} added successfully.", platform.Id);
            return CreatedAtAction(nameof(GetPlatformById), new { id = platform.Id }, platform);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddPlatform: An error occurred while adding the platform.");
            return StatusCode(500, "An error occurred while adding the platform.");
        }
    }

    [HttpPut("{id:guid}")]
    //[Authorize(Policy = "ReadWritePolicy")]
    public async Task<IActionResult> UpdatePlatform(Guid id, [FromBody] Platform updatedPlatform)
    {
        if (updatedPlatform == null)
        {
            _logger.LogWarning("UpdatePlatform: Received null platform data.");
            return BadRequest("Platform data cannot be null.");
        }

        var userId = HttpContext.Items["UserId"]?.ToString();
        if (userId == null)
        {
            _logger.LogWarning("UpdatePlatform: User not authenticated. UserId is null.");
            return Unauthorized("User not authenticated.");
        }

        updatedPlatform.Id = id;
        _logger.LogInformation("UpdatePlatform: User {UserId} is updating platform with ID {PlatformId}.", userId, id);

        try
        {
            await _platformService.UpdateAsync(id, updatedPlatform);
            _logger.LogInformation("UpdatePlatform: Platform with ID {PlatformId} updated successfully by User {UserId}.", id, userId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "UpdatePlatform: Platform with ID {PlatformId} not found.", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdatePlatform: An error occurred while updating the platform with ID {PlatformId}.", id);
            return StatusCode(500, "An error occurred while updating the platform.");
        }
    }

    /// <summary>
    /// Delete a platform if it has no associated emitters.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePlatform(Guid id)
    {
        _logger.LogInformation("DeletePlatform: Attempting to delete platform with ID {PlatformId}.", id);

        try
        {
            var (isDeleted, message) = await _platformService.DeletePlatformAsync(id);

            if (isDeleted)
            {
                _logger.LogInformation("DeletePlatform: Platform with ID {PlatformId} deleted successfully.", id);
                return NoContent();
            }

            _logger.LogWarning("DeletePlatform: Could not delete platform with ID {PlatformId}. Reason: {Reason}.", id, message);
            return BadRequest(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeletePlatform: An error occurred while deleting the platform with ID {PlatformId}.", id);
            return StatusCode(500, "An error occurred while deleting the platform.");
        }
    }

    [HttpGet("counts")]
    public async Task<IActionResult> GetPlatformCounts()
    {
        _logger.LogInformation("GetPlatformCounts: Fetching platform counts.");

        try
        {
            var total = await _platformService.GetCountAsync();
            var recent = await _platformService.GetRecentCountAsync(TimeSpan.FromDays(30)); // Last 30 days

            _logger.LogInformation("GetPlatformCounts: Successfully retrieved platform counts. Total: {Total}, Recent: {Recent}.", total, recent);
            return Ok(new { total, recent });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPlatformCounts: An error occurred while retrieving platform counts.");
            return StatusCode(500, "An error occurred while retrieving platform counts.");
        }
    }
}
