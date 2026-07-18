using n8neiritech.Domain.Common;

namespace n8neiritech.Domain.Entities;

public class SystemSetting : TenantEntity
{
    public Guid? StoreId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSecret { get; set; }
    public Tenant Tenant { get; set; } = null!;
}
