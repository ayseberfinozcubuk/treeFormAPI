// Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using tree_form_API.Services;
using tree_form_API.Models;
using System.Security.Claims;
using tree_form_API.Constants;
using MongoDB.Bson;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // GET: api/users - Retrieve all users (Admin-only access)
    [HttpGet]
    //[Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogInformation("Request received to get all users.");
        var users = await _userService.GetAllUsers();
        _logger.LogInformation($"Returning {users.Count} users.");
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id, [FromQuery] bool updatedDateOnly = false)
    {
        _logger.LogInformation("Request received to get all user by id: ", id);
        var user = await _userService.GetUserById(id);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        if (updatedDateOnly)
        {
            return Ok(new { UpdatedDate = user.UpdatedDate });
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
        _logger.LogInformation("Request received to post user registiration dto: ", userDto);
        //Console.WriteLine("userDto: ", userDto);
        var user = await _userService.AddUser(userDto);
        //Console.WriteLine("user: ", user);
        if (user == null)
        {
            return BadRequest("User could not be created.");
        }

        return Ok(user);
    }

    // DELETE: api/users/{id} - Delete a user by ID (Admin-only access)
    [HttpDelete("{id:guid}")]
    //[Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        _logger.LogInformation("Request received to delete a user by id: ", id);
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
        _logger.LogInformation("Request received to post sign-in with user login dto: ", loginDto);
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
        Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
        Response.Headers.Add("Access-Control-Allow-Credentials", "true");

        _logger.LogInformation($"Token set: {token}");

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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUserProfile(Guid id, [FromBody] UserUpdateDTO updateDto)
    {
        _logger.LogInformation("Request received to put an update ", updateDto , " on user by id: ", id);
        var updatedUser = await _userService.UpdateUser(id, updateDto);

        if (updatedUser == null)
        {
            return NotFound("User not found.");
        }

        return Ok(updatedUser);
    }

    [HttpPut("{id:guid}/change-password")]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDTO changePasswordDto)
    {
        _logger.LogInformation("Request received to put change password ", changePasswordDto , " on user by id: ", id);
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
    
    [HttpPut("{id:guid}/role")]
    //[Authorize(Policy = "AdminPolicy")] // Ensure only admins can update roles
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UserRoleUpdateDTO roleUpdateDto)
    {
        _logger.LogInformation("Request received to update user role with DTO: {RoleUpdateDto} for user ID: {Id}", roleUpdateDto, id);

        // Check if the user exists
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
        var success = await _userService.UpdateUserRole(id, roleUpdateDto);

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
        _logger.LogInformation("Request received to get user roles");
        var roles = new[] { UserRoles.Admin, UserRoles.Read, UserRoles.ReadWrite };
        return Ok(roles);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        _logger.LogInformation("Request received to log out");

        Response.Cookies.Delete("authToken");
        return Ok("Logged out successfully");
    }

    [HttpGet("{id:guid}/get-role")]
    public async Task<IActionResult> GetRole(Guid id)
    {
        _logger.LogInformation("Request received to get role on user by id: {Id}", id);

        if (!HttpContext.User.Identity.IsAuthenticated)
        {
            _logger.LogWarning("Unauthorized access attempt to GetRole.");
            return Unauthorized("User is not authenticated.");
        }

        // Retrieve user by id
        var user = await _userService.GetUserById(id);
        if (user == null)
        {
            _logger.LogInformation("User not found.");
            return NotFound("User not found.");
        }

        return Ok(new { role = user.Role });
    }

    [HttpOptions]
    [Route("{*path}")]
    public IActionResult Options()
    {
        Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
        Response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS, DELETE, PUT");
        Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
        Response.Headers.Add("Access-Control-Allow-Credentials", "true");

        return Ok();
    }

    [HttpGet("counts")]
    public async Task<IActionResult> GetUserCounts()
    {
        var total = await _userService.GetCountAsync();
        var recent = await _userService.GetRecentCountAsync(TimeSpan.FromDays(30)); // Last 30 days
        return Ok(new { total, recent });
    }

    [HttpGet("role-counts")]
    public async Task<IActionResult> GetRoleCounts()
    {
        _logger.LogInformation("Request received to get counts of roles in the database.");

        // Get raw role counts
        var rawRoleCounts = await _userService.GetRoleCounts();

        // Dynamically get role values from UserRoles class
        var roleValues = typeof(tree_form_API.Constants.UserRoles)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(field => field.IsLiteral || field.IsInitOnly) // Ensure static readonly or constants
            .ToDictionary(field => field.Name, field => (string)field.GetValue(null));

        // Map raw role keys to dynamically fetched values
        var roleCountsWithValues = rawRoleCounts.ToDictionary(
            role => roleValues.ContainsKey(role.Key) ? roleValues[role.Key] : role.Key, // Use value if available
            role => role.Value
        );

        return Ok(roleCountsWithValues);
    }
}
