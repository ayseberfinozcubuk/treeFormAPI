// Services/UserService.cs
using AutoMapper;
using MongoDB.Driver;
using tree_form_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace tree_form_API.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public UserService(IMongoCollection<User> userCollection, IMapper mapper, IConfiguration configuration)
        {
            _userCollection = userCollection;
            _mapper = mapper;
            _configuration = configuration;
        }

        // Get all users
        public async Task<List<UserResponseDTO>> GetAllUsers()
        {
            var users = await _userCollection.Find(_ => true).ToListAsync();
            return _mapper.Map<List<UserResponseDTO>>(users);
        }

        // Add user with specified role
        public async Task<UserResponseDTO?> AddUser(UserRegistrationDTO userDto)
        {
            var user = _mapper.Map<User>(userDto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            await _userCollection.InsertOneAsync(user);
            return _mapper.Map<UserResponseDTO>(user);
        }
        
        // Delete user by ID
        public async Task<bool> DeleteUser(string id)
        {
            var result = await _userCollection.DeleteOneAsync(u => u.Id == id);
            return result.DeletedCount > 0; // Returns true if deletion was successful
        }

        public async Task<string?> AuthenticateUser(UserLoginDTO loginDto)
        {
            // Retrieve the user by email
            var user = await _userCollection.Find(u => u.Email == loginDto.Email).FirstOrDefaultAsync();

            // Check if user exists
            if (user == null)
            {
                Console.WriteLine($"Authentication failed: No user found with email {loginDto.Email}");
                return null;
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                Console.WriteLine($"Authentication failed: Incorrect password for user {loginDto.Email}");
                return null;
            }

            // Generate a JWT token for the authenticated user
            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Claims for the token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role) // Include user role
            };

            // Calculate expiration dynamically
            var expiresInHours = Convert.ToDouble(jwtSettings["ExpiresInHours"]);
            var tokenExpiration = DateTime.UtcNow.AddHours(expiresInHours);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: tokenExpiration,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _userCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
        }
    
        public async Task<UserResponseDTO?> UpdateUser(string id, UserUpdateDTO updateDto)
        {
            var updateDefinition = Builders<User>.Update
                .Set(u => u.UserName, updateDto.UserName)
                .Set(u => u.Email, updateDto.Email);

            var result = await _userCollection.UpdateOneAsync(
                u => u.Id == id,
                updateDefinition);

            if (result.MatchedCount == 0)
            {
                return null; // No user found with given ID
            }

            // Fetch updated user
            var updatedUser = await _userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
            return updatedUser != null ? _mapper.Map<UserResponseDTO>(updatedUser) : null;
        }
    
        public async Task<bool> UpdatePassword(string userId, string newPassword)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            var updateDefinition = Builders<User>.Update.Set(u => u.PasswordHash, passwordHash);

            var result = await _userCollection.UpdateOneAsync(
                u => u.Id == userId,
                updateDefinition);

            return result.ModifiedCount > 0; // Returns true if update was successful
        }
    
        public async Task<bool> UpdateUserRole(string userId, string newRole)
        {
            var updateDefinition = Builders<User>.Update.Set(u => u.Role, newRole);
            var result = await _userCollection.UpdateOneAsync(
                u => u.Id == userId,
                updateDefinition);

            return result.ModifiedCount > 0; // Returns true if update was successful
        }

        public async Task<User?> GetUserById(string id)
        {
            return await _userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
        }
    
        public async Task<bool> UserUpdatedBy(string userId, string? updatedBy)
        {
            var updateDefinition = Builders<User>.Update.Set(u => u.UpdatedBy, updatedBy);

            var result = await _userCollection.UpdateOneAsync(
                u => u.Id == userId,
                updateDefinition
            );

            return result.ModifiedCount > 0; // Return true if the update was successful
        }
    }
}
