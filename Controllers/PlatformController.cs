using Microsoft.AspNetCore.Authorization;
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
        var platforms = await _platformService.GetAllPlatformsAsync();
        return Ok(platforms);
    }

    // GET: api/Platform/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPlatformById(Guid id)
    {
        var platform = await _platformService.GetPlatformByIdAsync(id);
        if (platform == null)
        {
            return NotFound();
        }
        return Ok(platform);
    }

    // POST: api/Platform
    [HttpPost]
    public async Task<IActionResult> AddPlatform([FromBody] Platform platform)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        await _platformService.AddPlatformAsync(platform);
        return CreatedAtAction(nameof(GetPlatformById), new { id = platform.Id }, platform);
    }

    [HttpPut("{id:guid}")]
    //[Authorize(Policy = "ReadWritePolicy")]
    public async Task<IActionResult> UpdatePlatform(Guid id, [FromBody] Platform updatedPlatform)
    {
        if (updatedPlatform == null)
        {
            return BadRequest("Platform data cannot be null.");
        }
        var userId = HttpContext.Items["UserId"]?.ToString();
        if (userId == null)
        {
            return Unauthorized("User not authenticated.");
        }
        updatedPlatform.Id = id;
        try
        {
            await _platformService.UpdateAsync(id, updatedPlatform);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // DELETE: api/Platform/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePlatform(Guid id)
    {
        var success = await _platformService.DeletePlatformAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
    
    [HttpGet("counts")]
    public async Task<IActionResult> GetPlatformCounts()
    {
        var total = await _platformService.GetCountAsync();
        var recent = await _platformService.GetRecentCountAsync(TimeSpan.FromDays(30)); // Last 30 days
        return Ok(new { total, recent });
    }
}
