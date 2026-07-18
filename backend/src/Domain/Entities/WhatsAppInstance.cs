using n8neiritech.Domain.Common;
using n8neiritech.Domain.Enums;

namespace n8neiritech.Domain.Entities;

public class WhatsAppInstance : TenantEntity
{
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InstanceName { get; set; } = string.Empty;
    public WhatsAppProviderType ProviderType { get; set; } = WhatsAppProviderType.EvolutionApi;
    public string? BaseUrl { get; set; }
    public string? ApiToken { get; set; }
    public string? WebhookSecret { get; set; }
    public string? ConnectedNumber { get; set; }
    public bool IsConnected { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastStatusCheck { get; set; }
    public string? LastError { get; set; }
    public Store Store { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
