// Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using tree_form_API.Services;
using tree_form_API.Models;
using tree_form_API.Constants;

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
        _logger.LogInformation("Fetching all users.");
        var users = await _userService.GetAllUsers();
        _logger.LogInformation("Successfully fetched {Count} users.", users.Count);
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id, [FromQuery] bool updatedDateOnly = false)
    {
        _logger.LogInformation("Fetching user with ID {UserId}.", id);
        var user = await _userService.GetUserById(id);

        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", id);
            return NotFound("User not found.");
        }

        _logger.LogInformation("Successfully fetched user with ID {UserId}.", id);

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
        _logger.LogInformation("Attempting to add a new user.");

        var user = await _userService.AddUser(userDto);
        if (user == null)
        {
            _logger.LogError("Failed to create user.");
            return BadRequest("User could not be created.");
        }

        _logger.LogInformation("User created successfully with ID {UserId}.", user.Id);
        return Ok(user);
    }

    // DELETE: api/users/{id} - Delete a user by ID (Admin-only access)
    [HttpDelete("{id:guid}")]
    //[Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        _logger.LogInformation("Attempting to delete user with ID {UserId}.", id);
        var success = await _userService.DeleteUser(id);

        if (!success)
        {
            _logger.LogWarning("Failed to delete user. User with ID {UserId} not found.", id);
            return NotFound("User not found.");
        }

        _logger.LogInformation("User with ID {UserId} deleted successfully.", id);
        return NoContent(); // 204 No Content
    }

    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] UserLoginDTO loginDto)
    {
        _logger.LogInformation("User sign-in attempt with email {Email}.", loginDto.Email);
        var token = await _userService.AuthenticateUser(loginDto);

        if (token == null)
        {
            _logger.LogWarning("Invalid credentials for email {Email}.", loginDto.Email);
            return Unauthorized("Invalid credentials.");
        }

        _logger.LogInformation("User authenticated successfully for email {Email}.", loginDto.Email);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddHours(72)
        };

        Response.Cookies.Append("authToken", token, cookieOptions);
        Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
        Response.Headers.Add("Access-Control-Allow-Credentials", "true");

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
        _logger.LogInformation("Change password request for user with ID {UserId}.", id);
        var user = await _userService.GetUserById(id);

        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", id);
            return NotFound("User not found.");
        }

        if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            _logger.LogWarning("Incorrect current password for user with ID {UserId}.", id);
            return Unauthorized("Current password is incorrect.");
        }

        if (BCrypt.Net.BCrypt.Verify(changePasswordDto.NewPassword, user.PasswordHash))
        {
            _logger.LogWarning("New password matches the current password for user with ID {UserId}.", id);
            return BadRequest("New password cannot be the same as the current password.");
        }

        if (changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
        {
            _logger.LogWarning("Password confirmation mismatch for user with ID {UserId}.", id);
            return BadRequest("New password and confirmation do not match.");
        }

        var success = await _userService.UpdatePassword(id, changePasswordDto.NewPassword);
        if (!success)
        {
            _logger.LogError("Failed to update password for user with ID {UserId}.", id);
            return StatusCode(500, "An error occurred while updating the password.");
        }

        _logger.LogInformation("Password updated successfully for user with ID {UserId}.", id);
        return Ok("Password updated successfully.");
    }
    
    [HttpPut("{id:guid}/role")]
    //[Authorize(Policy = "AdminPolicy")] // Ensure only admins can update roles
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UserRoleUpdateDTO roleUpdateDto)
    {
        _logger.LogInformation("UpdateUserRole: Request to update role for user ID {UserId} to {NewRole}.", id, roleUpdateDto.Role);

        var user = await _userService.GetUserById(id);
        if (user == null)
        {
            _logger.LogWarning("UpdateUserRole: User with ID {UserId} not found.", id);
            return NotFound("User not found.");
        }

        if (user.Role == roleUpdateDto.Role)
        {
            _logger.LogInformation("UpdateUserRole: User with ID {UserId} already has the role {Role}. No changes made.", id, user.Role);
            return Ok("The role is already set to the specified value. No changes made.");
        }

        var success = await _userService.UpdateUserRole(id, roleUpdateDto);

        if (!success)
        {
            _logger.LogError("UpdateUserRole: Failed to update role for user ID {UserId}.", id);
            return StatusCode(500, "An error occurred while updating the user role.");
        }

        _logger.LogInformation("UpdateUserRole: Role for user ID {UserId} updated to {NewRole} successfully.", id, roleUpdateDto.Role);
        return Ok("User role updated successfully.");
}

    [HttpGet("roles")]
    //[AllowAnonymous]
    public IActionResult GetUserRoles()
    {
        _logger.LogInformation("GetUserRoles: Fetching all user roles.");
        var roles = new[] { UserRoles.Admin, UserRoles.Read, UserRoles.ReadWrite };
        _logger.LogInformation("GetUserRoles: Successfully fetched {Count} roles.", roles.Length);
        return Ok(roles);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        _logger.LogInformation("Logout: User logged out successfully.");
        Response.Cookies.Delete("authToken");
        return Ok("Logged out successfully");
    }

    [HttpGet("{id:guid}/get-role")]
    public async Task<IActionResult> GetRole(Guid id)
    {
        _logger.LogInformation("GetRole: Attempt to fetch role for user ID {UserId}.", id);

        if (!HttpContext.User.Identity.IsAuthenticated)
        {
            _logger.LogWarning("GetRole: Unauthorized access attempt to fetch role for user ID {UserId}.", id);
            return Unauthorized("User is not authenticated.");
        }

        var user = await _userService.GetUserById(id);
        if (user == null)
        {
            _logger.LogWarning("GetRole: User with ID {UserId} not found.", id);
            return NotFound("User not found.");
        }

        _logger.LogInformation("GetRole: Successfully fetched role for user ID {UserId}. Role: {Role}.", id, user.Role);
        return Ok(new { role = user.Role });
    }

    [HttpOptions]
    [Route("{*path}")]
    public IActionResult Options()
    {
        _logger.LogInformation("Options: Preflight request received.");
        Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
        Response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS, DELETE, PUT");
        Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
        Response.Headers.Add("Access-Control-Allow-Credentials", "true");

        return Ok();
    }

    [HttpGet("counts")]
    public async Task<IActionResult> GetUserCounts()
    {
        _logger.LogInformation("GetUserCounts: Fetching user counts.");
        var total = await _userService.GetCountAsync();
        var recent = await _userService.GetRecentCountAsync(TimeSpan.FromDays(30));
        _logger.LogInformation("GetUserCounts: Total users: {Total}, recent users (last 30 days): {Recent}.", total, recent);
        return Ok(new { total, recent });
    }

    [HttpGet("role-counts")]
    public async Task<IActionResult> GetRoleCounts()
    {
        _logger.LogInformation("GetRoleCounts: Fetching role counts.");

        var rawRoleCounts = await _userService.GetRoleCounts();

        var roleValues = typeof(tree_form_API.Constants.UserRoles)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(field => field.IsLiteral || field.IsInitOnly)
            .ToDictionary(field => field.Name, field => (string)field.GetValue(null));

        var roleCountsWithValues = rawRoleCounts.ToDictionary(
            role => roleValues.ContainsKey(role.Key) ? roleValues[role.Key] : role.Key,
            role => role.Value
        );

        _logger.LogInformation("GetRoleCounts: Successfully fetched role counts.");
        return Ok(roleCountsWithValues);
    }
}
