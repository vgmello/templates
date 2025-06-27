// RequireRoleEndpointFilter.cs
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

public class RequireRoleEndpointFilter : IEndpointFilter
{
    private readonly string _requiredRole;

    public RequireRoleEndpointFilter(string requiredRole)
    {
        _requiredRole = requiredRole;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated == true)
        {
            return Results.Unauthorized();
        }

        if (!user.IsInRole(_requiredRole))
        {
            return Results.Forbid();
        }

        return await next(context);
    }
}
