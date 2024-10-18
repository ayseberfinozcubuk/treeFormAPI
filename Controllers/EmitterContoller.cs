using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.IO;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class EmittersController : ControllerBase
{
    private readonly EmitterRepository _emitterRepository;

    public EmittersController(EmitterRepository emitterRepository)
    {
        _emitterRepository = emitterRepository;
    }

    [HttpPost("upload-json")]
    public async Task<IActionResult> UploadEmitterJson(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Please upload a valid JSON file.");
        }

        using (var streamReader = new StreamReader(file.OpenReadStream()))
        {
            // Read file content as JSON string
            var jsonString = await streamReader.ReadToEndAsync();

            // Convert the JSON string into a BsonDocument
            var bsonDocument = BsonDocument.Parse(jsonString);

            // Insert the BsonDocument into MongoDB
            await _emitterRepository.CreateEmitterAsync(bsonDocument);
        }

        return Ok(new { message = "Emitter JSON uploaded and stored successfully!" });
    }
}
