using n8neiritech.Domain.Enums;

namespace n8neiritech.Application.DTOs;

public record MessageDto(Guid Id, Guid ConversationId, MessageDirection Direction, MessageType Type, string? Text, string? MediaUrl, MessageStatus Status, bool IsInternal, string? SentByUserId, DateTime CreatedAt);
public record ConversationListResponse(Guid Id, Guid StoreId, Guid CustomerId, string? CustomerName, string CustomerPhone, ConversationStatus Status, Guid? AssignedAgentId, string? AssignedAgentName, DateTime? LastMessageAt, int UnreadCount, bool AutomationPaused);
public record ConversationDetailResponse(Guid Id, Guid StoreId, Guid CustomerId, string? CustomerName, string CustomerPhone, ConversationStatus Status, Guid? AssignedAgentId, string? AssignedAgentName, bool AutomationPaused, string? SessionData, string? Notes, IReadOnlyCollection<MessageDto> Messages);
public record SendMessageRequest(string Text, MessageType Type = MessageType.Text, string? MediaUrl = null, bool IsInternal = false, string? QuotedMessageId = null);
public record AssignConversationRequest(Guid AgentId);
public record TransferConversationRequest(Guid AgentId);
public record SessionDataRequest(string SessionData);
