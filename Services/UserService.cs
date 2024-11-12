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
            var user = _mapper.Map<User>(userDto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            await _userCollection.InsertOneAsync(user);
            return _mapper.Map<UserResponseDTO>(user);
        }

        public async Task<string?> AuthenticateUser(UserLoginDTO loginDto)
        {
            var user = await _userCollection.Find(u => u.Email == loginDto.Email).FirstOrDefaultAsync();

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null; // Authentication failed
            }

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
                expires: DateTime.UtcNow.AddHours(Convert.ToDouble(jwtSettings["ExpiresInHours"])),
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
    }
}
