using n8neiritech.Domain.Common;

namespace n8neiritech.Domain.Entities;

public class AutomationRule : TenantEntity
{
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Trigger { get; set; } = string.Empty;
    public string? Conditions { get; set; }
    public string Actions { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
    public Store Store { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
