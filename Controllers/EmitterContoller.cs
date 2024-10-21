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

    [HttpGet]
    public async Task<IActionResult> GetAllEmitters()
    {
        var emitters = await _emitterService.GetAllAsync();
        var emittersDto = _mapper.Map<List<EmitterDto>>(emitters);
        return Ok(emittersDto);
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

        // Assign new Ids generically to Emitter and its nested entities
        AssignIdsGeneric(emitter);

        // Save to database
        await _emitterService.CreateAsync(emitter);

        return CreatedAtAction(nameof(Create), new { id = emitter.Id }, emitterDto);
    }

    // Generic method to assign Ids to Emitter and nested entities
    private void AssignIdsGeneric<T>(T entity)
    {
        if (entity == null) return;

        // Check if the entity has an Id property and assign a new Guid
        var idProperty = entity.GetType().GetProperty("Id");
        if (idProperty != null && idProperty.PropertyType == typeof(Guid))
        {
            idProperty.SetValue(entity, Guid.NewGuid());
        }

        // Iterate over all properties of the entity
        foreach (var property in entity.GetType().GetProperties())
        {
            if (typeof(IEnumerable<object>).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
            {
                // Recursively assign Ids for each item in the list
                var items = property.GetValue(entity) as IEnumerable<object>;
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        AssignIdsGeneric(item);  // Recursive call for nested entities
                    }
                }
            }
        }
    }
}
