public class UserRegistrationDTO
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; } // Plain text; will be hashed in the service
}