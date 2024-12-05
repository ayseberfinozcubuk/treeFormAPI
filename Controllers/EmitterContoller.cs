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

    public EmitterController(EmitterService emitterService)
    {
        _emitterService = emitterService;
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

        emitter.UpdatedBy = userId;
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
    public async Task<IActionResult> GetById(Guid id)
    {
        var emitter = await _emitterService.GetByIdAsync(id);

        if (emitter == null)
        {
            return NotFound($"Emitter with ID {id} not found.");
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
        updatedEmitter.UpdatedBy = userId;

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

    [HttpPatch("emitter-updatedby")]
    [Authorize(Policy = "ReadWritePolicy")]
    public async Task<IActionResult> UpdateUpdatedBy([FromBody] EmitterUpdatedByDTO dto)
    {
        if (dto == null)
        {
            return BadRequest("DTO cannot be null.");
        }

        try
        {
            await _emitterService.EmitterUpdatedByAsync(dto.Id, dto.UpdatedBy);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}