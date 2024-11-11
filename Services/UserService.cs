// Services/UserService.cs
using AutoMapper;
using MongoDB.Driver;
using tree_form_API.Models;

namespace tree_form_API.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMapper _mapper;

        public UserService(IMongoCollection<User> userCollection, IMapper mapper)
        {
            _userCollection = userCollection;
            _mapper = mapper;
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

        public async Task<UserResponseDTO> AuthenticateUser(UserLoginDTO loginDto)
        {
            // Find user by email
            var user = await _userCollection.Find(u => u.Email == loginDto.Email).FirstOrDefaultAsync();

            // Validate password
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null; // Authentication failed
            }

            // Map user entity to response DTO and return it
            return _mapper.Map<UserResponseDTO>(user);
        }
    }
}
