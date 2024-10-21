using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using tree_form_API.Dtos;
using tree_form_API.Models;

[ApiController]
[Route("api/[controller]")]
public class EmitterController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly EmitterService _emitterService;

    public EmitterController(IMapper mapper, EmitterService emitterService)
    {
        _mapper = mapper;
        _emitterService = emitterService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(EmitterDto emitterDto)
    {
        if (emitterDto == null)
        {
            return BadRequest("Emitter data cannot be null.");
        }

        // Map DTO to domain model
        var emitter = _mapper.Map<Emitter>(emitterDto);

        // Generate new Id for Emitter and nested entities
        emitter.Id = Guid.NewGuid();

        foreach (var mode in emitter.Modes)
        {
            mode.Id = Guid.NewGuid();
            mode.EmitterId = emitter.Id;

            foreach (var beam in mode.Beams)
            {
                beam.Id = Guid.NewGuid();
                beam.EmitterModeId = mode.Id;

                foreach (var dwell in beam.DwellDurationValues)
                {
                    dwell.Id = Guid.NewGuid();
                    dwell.EmitterModeBeamId = beam.Id;
                }

                foreach (var sequence in beam.Sequences)
                {
                    sequence.Id = Guid.NewGuid();
                    sequence.EmitterModeBeamId = beam.Id;

                    foreach (var firingOrder in sequence.FiringOrders)
                    {
                        firingOrder.Id = Guid.NewGuid();
                        firingOrder.EmitterModeBeamPositionSequenceId = sequence.Id;
                    }
                }
            }
        }

        // Save to database
        await _emitterService.CreateAsync(emitter);

        return CreatedAtAction(nameof(Create), new { id = emitter.Id }, emitterDto);
    }
}
