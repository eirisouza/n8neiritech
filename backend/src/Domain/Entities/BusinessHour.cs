using n8neiritech.Domain.Common;

namespace n8neiritech.Domain.Entities;

public class BusinessHour : TenantEntity
{
    public Guid StoreId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsOpen { get; set; } = true;
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public Store Store { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
