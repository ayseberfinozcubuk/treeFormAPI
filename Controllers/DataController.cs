using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly DataService _dataService;

    public DataController(DataService dataService)
    {
        _dataService = dataService;
    }

    [HttpPost]
    public async Task<IActionResult> PostData([FromBody] InputDataDto inputData)
    {
        if (inputData == null)
        {
            return BadRequest("Invalid input data.");
        }

        await _dataService.InsertDataAsync(inputData);
        return Ok("Data inserted successfully.");
    }
}
