using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using tree_form_API.Dtos;
using tree_form_API.Models;
using tree_form_API.Services;

namespace tree_form_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmitterController : ControllerBase
    {
        private readonly EmitterService _emitterService;
        private readonly IMapper _mapper;

        public EmitterController(EmitterService emitterService, IMapper mapper)
        {
            _emitterService = emitterService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmitterDto emitterDto)
        {
            var emitter = _mapper.Map<Emitter>(emitterDto); // Map DTO to Domain
            await _emitterService.CreateAsync(emitter);
            return CreatedAtAction(nameof(Create), new { id = emitter.Id }, emitterDto);
        }
    }
}
