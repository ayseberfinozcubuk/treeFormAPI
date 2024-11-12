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

        var user = await _userService.GetUserByEmail(loginDto.Email);
        var userResponse = new UserResponseDTO
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email
        };

        return Ok(new { User = userResponse, Token = token });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserProfile(string id, [FromBody] UserUpdateDTO updateDto)
    {
        var updatedUser = await _userService.UpdateUser(id, updateDto);

        if (updatedUser == null)
        {
            return NotFound("User not found.");
        }

        return Ok(updatedUser);
    }

    [HttpPut("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordDTO changePasswordDto)
    {
        var user = await _userService.GetUserById(id); // Implement this method if not present
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            return Unauthorized("Current password is incorrect.");
        }

        // Check new password confirmation
        if (changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
        {
            return BadRequest("New password and confirmation do not match.");
        }

        // Update password
        var success = await _userService.UpdatePassword(id, changePasswordDto.NewPassword);
        if (!success)
        {
            return StatusCode(500, "An error occurred while updating the password.");
        }

        return Ok("Password updated successfully.");
    }
}
