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
        var path = context.Request.Path.Value?.ToLower();

        // Skip validation for preflight OPTIONS requests
        if (context.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Skip validation for public endpoints like /signin
        if (path != null && path.StartsWith("/api/users/signin"))
        {
            await _next(context);
            return;
        }

        string token = context.Request.Cookies.ContainsKey("authToken")
            ? context.Request.Cookies["authToken"]
            : context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (string.IsNullOrEmpty(token))
        {
            logger.LogWarning("Token is missing.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: No token provided.");
            return;
        }

        try
        {
            var userId = ValidateToken(token, logger);
            context.Items["UserId"] = userId;
        }
        catch
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Invalid token.");
            return;
        }

        await _next(context);
    }

    private string ValidateToken(string token, ILogger logger)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero
        };

        // Validate the token
        var principal = tokenHandler.ValidateToken(token, parameters, out var validatedToken);

        if (validatedToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token.");
        }

        // Retrieve the UserId from claims
        return principal.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
    }
}
