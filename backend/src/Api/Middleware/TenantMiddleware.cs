using n8neiritech.Application.Interfaces;

namespace n8neiritech.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentUser currentUser)
    {
        if (currentUser.IsAuthenticated)
        {
            context.Items["TenantId"] = currentUser.TenantId;
            context.Items["StoreId"] = currentUser.StoreId;
            context.Items["UserId"] = currentUser.Id;
        }

        await _next(context);
    }
}
