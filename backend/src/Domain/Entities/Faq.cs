using n8neiritech.Domain.Common;

namespace n8neiritech.Domain.Entities;

public class Faq : TenantEntity
{
    public Guid StoreId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public Store Store { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
