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

        public async Task<UserResponseDTO> RegisterUser(UserRegistrationDTO userDto)
        {
            // Map DTO to User entity
            var user = _mapper.Map<User>(userDto);
            
            // Hash password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            // Insert user into collection
            await _userCollection.InsertOneAsync(user);

            // Map created User entity back to response DTO and return it
            return _mapper.Map<UserResponseDTO>(user);
        }

        public async Task<string?> AuthenticateUser(UserLoginDTO loginDto)
        {
            // Find user by email
            var user = await _userCollection.Find(u => u.Email == loginDto.Email).FirstOrDefaultAsync();

            // Validate password
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null; // Authentication failed
            }

            // Generate JWT token
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
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
