using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using n8neiritech.Application.Interfaces;
using n8neiritech.Domain.Enums;

namespace n8neiritech.Infrastructure.Services;

public class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid Id => ParseGuid(ClaimTypes.NameIdentifier, ClaimTypes.Name, ClaimTypes.Sid, "sub");
    public Guid TenantId => ParseGuid("tenant_id");
    public Guid? StoreId => ParseNullableGuid("store_id");
    public string Email => User?.FindFirstValue(ClaimTypes.Email) ?? User?.FindFirstValue("email") ?? string.Empty;
    public UserRole Role => Enum.TryParse<UserRole>(User?.FindFirstValue(ClaimTypes.Role), true, out var role) ? role : UserRole.Viewer;
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    private Guid ParseGuid(params string[] claimTypes)
    {
        var value = claimTypes.Select(type => User?.FindFirstValue(type)).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        return Guid.TryParse(value, out var guid) ? guid : Guid.Empty;
    }

    private Guid? ParseNullableGuid(string claimType)
    {
        var value = User?.FindFirstValue(claimType);
        return Guid.TryParse(value, out var guid) ? guid : null;
    }
}
