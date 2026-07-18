using n8neiritech.Domain.Common;
using n8neiritech.Domain.Enums;

namespace n8neiritech.Domain.Entities;

public class Conversation : TenantEntity
{
    public Guid StoreId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? WhatsAppInstanceId { get; set; }
    public Guid? AssignedAgentId { get; set; }
    public ConversationStatus Status { get; set; } = ConversationStatus.New;
    public string? SessionData { get; set; }
    public bool AutomationPaused { get; set; }
    public DateTime? AutomationPausedUntil { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    public string? Notes { get; set; }
    public Store Store { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public WhatsAppInstance? WhatsAppInstance { get; set; }
    public User? AssignedAgent { get; set; }
    public ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
}
