using n8neiritech.Domain.Common;

namespace n8neiritech.Domain.Entities;

public class Store : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; } = "BR";
    public bool IsActive { get; set; } = true;
    public string? BusinessType { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public ICollection<WhatsAppInstance> WhatsAppInstances { get; set; } = new List<WhatsAppInstance>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<BusinessHour> BusinessHours { get; set; } = new List<BusinessHour>();
    public ICollection<AutomationRule> AutomationRules { get; set; } = new List<AutomationRule>();
}
