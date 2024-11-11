using Microsoft.AspNetCore.Mvc;
using tree_form_API.Models;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using tree_form_API.Services;

[Authorize]
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
    public async Task<IActionResult> Create([FromBody] Emitter emitter)
    {
        if (emitter == null)
        {
            return BadRequest("Emitter data cannot be null.");
        }

        // Save to database directly with IDs provided by the frontend
        await _emitterService.CreateAsync(emitter);

        return CreatedAtAction(nameof(GetById), new { id = emitter.Id }, emitter); // Use GetById to provide the location of the created resource
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var emitters = await _emitterService.GetAllAsync();
        return Ok(emitters);
    }

    [HttpGet("{id:guid}")]
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
    public async Task<IActionResult> Update(Guid id, [FromBody] Emitter updatedEmitter)
    {
        if (updatedEmitter == null)
        {
            return BadRequest("Emitter data cannot be null.");
        }

        // Ensure the correct Id is assigned
        updatedEmitter.Id = id;

        try
        {
            await _emitterService.UpdateAsync(id, updatedEmitter);
            return NoContent(); // Returns 204 No Content on successful update
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message); // Returns 404 if the Emitter is not found
        }
    }

    [HttpDelete("{id:guid}")]
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
}
