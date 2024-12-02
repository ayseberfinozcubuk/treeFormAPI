// Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using tree_form_API.Services;
using tree_form_API.Models;
using System.Security.Claims;
using tree_form_API.Constants;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    // GET: api/users - Retrieve all users (Admin-only access)
    [HttpGet]
    //[Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsers();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userService.GetUserById(id);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        return Ok(new UserResponseDTO
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role
        });
    }

    // POST: api/users/add - Add a new user (Admin-only access)
    [HttpPost("add")]
    //[Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> AddUser([FromBody] UserRegistrationDTO userDto)
    {
        Console.WriteLine("userDto: ", userDto);
        var user = await _userService.AddUser(userDto);
        Console.WriteLine("user: ", user);
        if (user == null)
        {
            return BadRequest("User could not be created.");
        }

        return Ok(user);
    }

    // DELETE: api/users/{id} - Delete a user by ID (Admin-only access)
    [HttpDelete("{id}")]
    //[Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var success = await _userService.DeleteUser(id);
        if (!success)
        {
            return NotFound("User not found.");
        }

        return NoContent(); // 204 No Content
    }

    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] UserLoginDTO loginDto)
    {
        var token = await _userService.AuthenticateUser(loginDto);

        if (token == null)
        {
            return Unauthorized("Invalid credentials.");
        }

        // Set the token as a secure HTTP-only cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // Prevent JavaScript access
            Secure = false,   // Send over HTTPS only; set to false during development if not using HTTPS
            SameSite = SameSiteMode.Lax, // Adjust to None if cross-origin requests are needed
            Expires = DateTime.UtcNow.AddHours(72) // Set token expiration
        };

        Response.Cookies.Append("authToken", token, cookieOptions);

        var user = await _userService.GetUserByEmail(loginDto.Email);
        var userResponse = new UserResponseDTO
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role,
        };

        return Ok(new { User = userResponse });
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
        var user = await _userService.GetUserById(id); // Retrieve the user
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            return Unauthorized("Current password is incorrect.");
        }

        // Prevent the new password from being the same as the current password
        if (BCrypt.Net.BCrypt.Verify(changePasswordDto.NewPassword, user.PasswordHash))
        {
            return BadRequest("New password cannot be the same as the current password.");
        }

        // Check new password confirmation
        if (changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
        {
            return BadRequest("New password and confirmation do not match.");
        }

        // Update the password
        var success = await _userService.UpdatePassword(id, changePasswordDto.NewPassword);
        if (!success)
        {
            return StatusCode(500, "An error occurred while updating the password.");
        }

        return Ok("Password updated successfully.");
    }
    
    [HttpPut("{id}/role")]
    //[Authorize(Policy = "AdminPolicy")] // Ensure only admins can update roles
    public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UserRoleUpdateDTO roleUpdateDto)
    {
        // Get the current user
        var user = await _userService.GetUserById(id);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Check if the new role is the same as the current role
        if (user.Role == roleUpdateDto.Role)
        {
            return Ok("The role is already set to the specified value. No changes made.");
        }

        // Proceed to update the role
        var success = await _userService.UpdateUserRole(id, roleUpdateDto.Role);

        if (!success)
        {
            return StatusCode(500, "An error occurred while updating the user role.");
        }

        return Ok("User role updated successfully.");
    }

    [HttpGet("roles")]
    //[AllowAnonymous]
    public IActionResult GetUserRoles()
    {
        var roles = new[] { UserRoles.Admin, UserRoles.Read, UserRoles.ReadWrite };
        return Ok(roles);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Clear the authToken cookie
        Response.Cookies.Delete("authToken");
        return Ok("Logged out successfully");
    }

    [HttpGet("check-auth")]
    public IActionResult CheckAuth()
    {
        var authCookie = Request.Cookies["authToken"];
        if (string.IsNullOrEmpty(authCookie))
        {
            return Unauthorized("Authentication token is missing.");
        }

        //var isValid = ValidateJwtToken(authCookie); // Implement your token validation logic
        //if (!isValid)
        //{
        //    return Unauthorized("Invalid or expired token.");
        //}

        return Ok("Authenticated");
    }

    [HttpGet("{id}/get-role")]
    public IActionResult GetRole()
    {
        if (HttpContext.User.Identity.IsAuthenticated)
        {
            var role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            return Ok(new { role });
        }
        return Unauthorized();
    }

    [HttpPatch("user-updatedby")]
    public async Task<IActionResult> UserUpdatedBy([FromBody] UserUpdatedByDTO dto)
    {
        if (dto == null)
        {
            return BadRequest("DTO cannot be null.");
        }

        var user = await _userService.GetUserById(dto.Id);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        try
        {
            await _userService.UserUpdatedBy(dto.Id, dto.UpdatedBy);
            return NoContent(); // 204 No Content indicates success without a response body
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while updating UpdatedBy: {ex.Message}");
        }
    }
}
