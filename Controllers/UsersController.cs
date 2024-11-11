// Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using tree_form_API.Services;
using tree_form_API.Models;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] UserRegistrationDTO userDto)
    {
        var user = await _userService.RegisterUser(userDto);
        if (user == null)
        {
            return BadRequest("User could not be created.");
        }
        
        var token = await _userService.AuthenticateUser(new UserLoginDTO
        {
            Email = user.Email,
            Password = userDto.Password
        });

        return Ok(new { user, Token = token });
    }

    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] UserLoginDTO loginDto)
    {
        var token = await _userService.AuthenticateUser(loginDto);

        if (token == null)
        {
            return Unauthorized("Invalid credentials.");
        }

        return Ok(new { Token = token });
    }
}
