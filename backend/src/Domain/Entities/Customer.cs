using n8neiritech.Domain.Common;

namespace n8neiritech.Domain.Entities;

public class Customer : TenantEntity
{
    public Guid StoreId { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public DateTime? BirthDate { get; set; }
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }
    public bool ConsentMarketing { get; set; }
    public string? Notes { get; set; }
    public DateTime? LastInteractionAt { get; set; }
    public Store Store { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
