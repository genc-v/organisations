using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace cmsOrg.API.Middlewares;

public class JwtValidationMiddleware(RequestDelegate next, IConfiguration configuration)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            await next(context);
            return;
        }

        var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"]!));
            var handler = new JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidAudience = configuration["JwtSettings:Audience"],
                IssuerSigningKey = key
            }, out _);

            context.User = principal;
            await next(context);
        }
        catch (SecurityTokenException)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
    }
}
