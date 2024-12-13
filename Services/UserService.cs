// Services/UserService.cs
using AutoMapper;
using MongoDB.Driver;
using tree_form_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace tree_form_API.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;

        public UserService(IMongoCollection<User> userCollection, IMapper mapper, IConfiguration configuration, ILogger<UserService> logger)
        {
            _userCollection = userCollection;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<UserResponseDTO>> GetAllUsers()
        {
            _logger.LogInformation("Fetching all users.");
            var users = await _userCollection.Find(_ => true).ToListAsync();
            return _mapper.Map<List<UserResponseDTO>>(users);
        }

        public async Task<UserResponseDTO?> AddUser(UserRegistrationDTO userDto)
        {
            _logger.LogInformation("Adding a new user with email {Email}.", userDto.Email);

            var user = _mapper.Map<User>(userDto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            user.CreatedDate = DateTime.UtcNow;

            await _userCollection.InsertOneAsync(user);
            _logger.LogInformation("User with email {Email} added successfully.", userDto.Email);

            return _mapper.Map<UserResponseDTO>(user);
        }
        
        public async Task<bool> DeleteUser(Guid id)
        {
            _logger.LogInformation("Attempting to delete user with ID {UserId}.", id);
            var result = await _userCollection.DeleteOneAsync(u => u.Id == id);

            if (result.DeletedCount > 0)
            {
                _logger.LogInformation("User with ID {UserId} deleted successfully.", id);
                return true;
            }

            _logger.LogWarning("User with ID {UserId} not found for deletion.", id);
            return false;
        }

        public async Task<string?> AuthenticateUser(UserLoginDTO loginDto)
        {
            _logger.LogInformation("Authentication attempt for email {Email}.", loginDto.Email);

            var user = await _userCollection.Find(u => u.Email == loginDto.Email).FirstOrDefaultAsync();

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Authentication failed for email {Email}.", loginDto.Email);
                return null;
            }

            _logger.LogInformation("Authentication successful for email {Email}.", loginDto.Email);
            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(Convert.ToDouble(jwtSettings["ExpiresInHours"])),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _userCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
        }
    
        public async Task<UserResponseDTO?> UpdateUser(Guid id, UserUpdateDTO updateDto)
        {
            _logger.LogInformation("Updating user with ID {UserId}.", id);

            var updateDefinition = Builders<User>.Update
                .Set(u => u.UserName, updateDto.UserName)
                .Set(u => u.Email, updateDto.Email);

            var result = await _userCollection.UpdateOneAsync(u => u.Id == id, updateDefinition);

            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("User with ID {UserId} not found for update.", id);
                return null;
            }

            _logger.LogInformation("User with ID {UserId} updated successfully.", id);
            var updatedUser = await _userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
            return updatedUser != null ? _mapper.Map<UserResponseDTO>(updatedUser) : null;
        }
    
        public async Task<bool> UpdatePassword(Guid userId, string newPassword)
        {
            _logger.LogInformation("Updating password for user with ID {UserId}.", userId);

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            var updateDefinition = Builders<User>.Update.Set(u => u.PasswordHash, passwordHash);

            var result = await _userCollection.UpdateOneAsync(u => u.Id == userId, updateDefinition);

            if (result.ModifiedCount > 0)
            {
                _logger.LogInformation("Password updated successfully for user with ID {UserId}.", userId);
                return true;
            }

            _logger.LogWarning("Failed to update password for user with ID {UserId}.", userId);
            return false;
        }
    
        public async Task<bool> UpdateUserRole(Guid id, UserRoleUpdateDTO roleUpdateDto)
        {
            _logger.LogInformation("Updating role for user with ID {UserId}.", id);
            if (roleUpdateDto == null)
            {
                throw new ArgumentNullException(nameof(roleUpdateDto), "Role update DTO cannot be null.");
            }


            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var updateDefinition = Builders<User>.Update
                .Set(u => u.Role, roleUpdateDto.Role)
                .Set(u => u.UpdatedBy, roleUpdateDto.UpdatedBy)
                .Set(u => u.UpdatedDate, DateTime.UtcNow);

            try
            {
                var result = await _userCollection.UpdateOneAsync(filter, updateDefinition);

                if (result.MatchedCount == 0)
                {
                    _logger.LogWarning("User with ID {UserId} not found for role update.", id);
                    return false; // No matching user to update
                }

                if (result.ModifiedCount > 0)
                {
                    return true;
                }

                _logger.LogInformation("Role for user with ID {UserId} updated successfully.", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the role for user ID: {UserId}", roleUpdateDto.UpdatedBy);
                throw; 
            }
        }

        public async Task<User?> GetUserById(Guid id)
        {
            try
            {
                return await _userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching user with ID {UserId}.", id);
                throw;
            }
        }
    
        public async Task<long> GetCountAsync()
        {
            _logger.LogInformation("Counting total users.");
            return await _userCollection.CountDocumentsAsync(_ => true);
        }
    
        public async Task<long> GetRecentCountAsync(TimeSpan timeSpan)
        {
            var recentDate = DateTime.UtcNow.Subtract(timeSpan);
            _logger.LogInformation("Counting users created since {RecentDate}.", recentDate);
            var filter = Builders<User>.Filter.Gte(e => e.CreatedDate, recentDate);
            return await _userCollection.CountDocumentsAsync(filter);
        }
    
        public async Task<Dictionary<string, long>> GetRoleCounts()
        {
            _logger.LogInformation("Fetching role counts.");
            var roleCounts = await _userCollection.Aggregate()
                .Group(
                    u => u.Role,
                    g => new { Role = g.Key, Count = g.Count() }
                )
                .ToListAsync();

            return roleCounts.ToDictionary(rc => rc.Role, rc => (long)rc.Count);
        }
    }
}
