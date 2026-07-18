using n8neiritech.Domain.Common;
using n8neiritech.Domain.Enums;

namespace n8neiritech.Domain.Entities;

public class User : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Operator;
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid? StoreId { get; set; }
    public Store? Store { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
