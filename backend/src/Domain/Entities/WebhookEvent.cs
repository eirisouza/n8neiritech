using n8neiritech.Domain.Common;

namespace n8neiritech.Domain.Entities;

public class WebhookEvent : TenantEntity
{
    public Guid StoreId { get; set; }
    public Guid? WhatsAppInstanceId { get; set; }
    public string ExternalEventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string RawPayload { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public bool Processed { get; set; }
    public int RetryCount { get; set; }
    public string? ProcessingError { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public Store Store { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
