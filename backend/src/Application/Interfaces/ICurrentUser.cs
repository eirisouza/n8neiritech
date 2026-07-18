using n8neiritech.Domain.Enums;

namespace n8neiritech.Application.Interfaces;

public interface ICurrentUser
{
    Guid Id { get; }
    Guid TenantId { get; }
    Guid? StoreId { get; }
    string Email { get; }
    UserRole Role { get; }
    bool IsAuthenticated { get; }
}
