using n8neiritech.Domain.Common;
using n8neiritech.Domain.Enums;

namespace n8neiritech.Domain.Entities;

public class ConversationMessage : BaseEntity
{
    public Guid ConversationId { get; set; }
    public string? ExternalMessageId { get; set; }
    public MessageDirection Direction { get; set; }
    public MessageType Type { get; set; } = MessageType.Text;
    public string? Text { get; set; }
    public string? MediaUrl { get; set; }
    public string? MediaMimeType { get; set; }
    public MessageStatus Status { get; set; } = MessageStatus.Sent;
    public bool IsInternal { get; set; }
    public string? SentByUserId { get; set; }
    public string? CorrelationId { get; set; }
    public string? RawPayload { get; set; }
    public Conversation Conversation { get; set; } = null!;
}
