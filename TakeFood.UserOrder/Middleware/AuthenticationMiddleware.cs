using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using StoreService.Service;
using System.IdentityModel.Tokens.Jwt;

namespace StoreService.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var id = context.Request.Headers.TryGetValue("Uid", out StringValues sv);
            context.Items["Id"] = sv.ToString();
            await _next(context);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
