using Microsoft.AspNetCore.Mvc;
using tree_form_API.Models;
using tree_form_API.Services;

namespace tree_form_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlatformController : ControllerBase
    {
        private readonly PlatformService _platformService;

        public PlatformController(PlatformService platformService)
        {
            _platformService = platformService;
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
    
        [HttpPatch("platform-updatedby")]
        //[Authorize(Policy = "ReadWritePolicy")]
        public async Task<IActionResult> UpdateUpdatedBy([FromBody] PlatformUpdatedByDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("DTO cannot be null.");
            }

            try
            {
                await _platformService.PlatformUpdatedByAsync(dto.Id, dto.UpdatedBy);
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
}
