using n8neiritech.Domain.Common;

namespace n8neiritech.Domain.Entities;

public class Promotion : TenantEntity
{
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal? MinOrderValue { get; set; }
    public string? ApplicableProductIds { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public bool IsActive { get; set; } = true;
    public Store Store { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
