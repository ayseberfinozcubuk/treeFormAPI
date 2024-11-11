public class UserLoginDTO
{
    public string Email { get; set; }
    public string Password { get; set; } // Plain text; will be verified in the service
}