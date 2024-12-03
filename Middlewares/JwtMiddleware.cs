using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using tree_form_API.Services;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public JwtMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context, UserService userService, ILogger<JwtMiddleware> logger)
    {
        var token = context.Request.Cookies["authToken"]; // Or retrieve from Authorization header

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                logger.LogInformation("Attempting to validate token.");
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

                // Attach the user ID to the context
                context.Items["UserId"] = userId;
                logger.LogInformation($"Token validated successfully for UserId: {userId}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Token validation failed: {ex.Message}");
            }
        }
        await _next(context);
    }
}
