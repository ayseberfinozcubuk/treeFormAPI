using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Token is missing.");
            return;
        }

        try
        {
            var jwtSettings = context.RequestServices.GetService<IConfiguration>().GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            // Extract user claims from token and add to HttpContext
            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            context.Items["UserId"] = userId;
        }
        catch
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid or expired token.");
            return;
        }

        await _next(context);
    }
}
