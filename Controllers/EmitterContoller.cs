using Microsoft.AspNetCore.Mvc;
using tree_form_API.Models;
using Microsoft.AspNetCore.Authorization;
using tree_form_API.Services;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmitterController : ControllerBase
{
    private readonly EmitterService _emitterService;
    private readonly ILogger<UsersController> _logger;

    public EmitterController(EmitterService emitterService, ILogger<UsersController> logger)
    {
        _emitterService = emitterService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Policy = "ReadWritePolicy")]
    public async Task<IActionResult> Create([FromBody] Emitter emitter)
    {
        if (emitter == null)
        {
            _logger.LogWarning("Create: Received null emitter data.");
            return BadRequest("Emitter data cannot be null.");
        }

        var userId = HttpContext.Items["UserId"]?.ToString();
        if (userId == null)
        {
            _logger.LogWarning("Create: User not authenticated. UserId is null.");
            return Unauthorized("User not authenticated.");
        }

        _logger.LogInformation("Create: User {UserId} is creating a new emitter.", userId);

        try
        {
            await _emitterService.CreateAsync(emitter);
            _logger.LogInformation("Create: Emitter created successfully with ID {EmitterId} by User {UserId}.", emitter.Id, userId);
            return CreatedAtAction(nameof(GetById), new { id = emitter.Id }, emitter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create: An error occurred while creating the emitter.");
            return StatusCode(500, "An error occurred while creating the emitter.");
        }
    }

    //[AllowAnonymous]
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("GetAll: Retrieving all emitters.");
        var emitters = await _emitterService.GetAllAsync();
        return Ok(emitters);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] bool updatedDateOnly = false)
    {
        _logger.LogInformation("GetById: Retrieving emitter with ID {EmitterId}.", id);

        var emitter = await _emitterService.GetByIdAsync(id);

        if (emitter == null)
        {
            _logger.LogWarning("GetById: Emitter with ID {EmitterId} not found.", id);
            return NotFound($"Emitter with ID {id} not found.");
        }

        _logger.LogInformation("GetById: Successfully retrieved emitter with ID {EmitterId}.", id);

        if (updatedDateOnly)
        {
            return Ok(new { UpdatedDate = emitter.UpdatedDate });
        }

        return Ok(emitter);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ReadWritePolicy")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Emitter updatedEmitter)
    {
        if (updatedEmitter == null)
        {
            _logger.LogWarning("Update: Received null emitter data.");
            return BadRequest("Emitter data cannot be null.");
        }

        var userId = HttpContext.Items["UserId"]?.ToString();
        if (userId == null)
        {
            _logger.LogWarning("Update: User not authenticated. UserId is null.");
            return Unauthorized("User not authenticated.");
        }

        updatedEmitter.Id = id;

        try
        {
            await _emitterService.UpdateAsync(id, updatedEmitter);
            _logger.LogInformation("Update: Emitter with ID {EmitterId} updated successfully by User {UserId}.", id, userId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Update: Emitter with ID {EmitterId} not found.", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update: An error occurred while updating the emitter with ID {EmitterId}.", id);
            return StatusCode(500, "An error occurred while updating the emitter.");
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _emitterService.DeleteAsync(id);
            _logger.LogInformation("Delete: Emitter with ID {EmitterId} deleted successfully.", id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Delete: Emitter with ID {EmitterId} not found.", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete: An error occurred while deleting the emitter with ID {EmitterId}.", id);
            return StatusCode(500, "An error occurred while deleting the emitter.");
        }
    }

    [HttpGet("counts")]
    public async Task<IActionResult> GetEmitterCounts()
    {
        _logger.LogInformation("GetEmitterCounts: Retrieving emitter counts.");

        try
        {
            var total = await _emitterService.GetCountAsync();
            var recent = await _emitterService.GetRecentCountAsync(TimeSpan.FromDays(30)); // Last 30 days
            _logger.LogInformation("GetEmitterCounts: Successfully retrieved emitter counts. Total: {Total}, Recent: {Recent}.", total, recent);
            return Ok(new { total, recent });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetEmitterCounts: An error occurred while retrieving emitter counts.");
            return StatusCode(500, "An error occurred while retrieving emitter counts.");
        }
    }
}
