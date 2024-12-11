using Microsoft.AspNetCore.Mvc;
using tree_form_API.Models;
using System;
using System.Threading.Tasks;
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
            return BadRequest("Emitter data cannot be null.");
        }

        // Debug or log the UserId value
        var userId = HttpContext.Items["UserId"]?.ToString();
        if (userId == null)
        {
            Console.WriteLine("Debug: UserId is null in HttpContext.Items");
            return Unauthorized("User not authenticated.");
        }

        Console.WriteLine($"Debug: UserId found in HttpContext.Items: {userId}");

        await _emitterService.CreateAsync(emitter);

        return CreatedAtAction(nameof(GetById), new { id = emitter.Id }, emitter);
    }

    //[AllowAnonymous]
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var emitters = await _emitterService.GetAllAsync();
        return Ok(emitters);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] bool updatedDateOnly = false)
    {
        var emitter = await _emitterService.GetByIdAsync(id);

        if (emitter == null)
        {
            return NotFound($"Emitter with ID {id} not found.");
        }
        
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
            return BadRequest("Emitter data cannot be null.");
        }

        var userId = HttpContext.Items["UserId"]?.ToString();
        if (userId == null)
        {
            return Unauthorized("User not authenticated.");
        }

        updatedEmitter.Id = id;

        try
        {
            await _emitterService.UpdateAsync(id, updatedEmitter);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _emitterService.DeleteAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("counts")]
    public async Task<IActionResult> GetEmitterCounts()
    {
        var total = await _emitterService.GetCountAsync();
        var recent = await _emitterService.GetRecentCountAsync(TimeSpan.FromDays(30)); // Last 30 days
        return Ok(new { total, recent });
    }
}