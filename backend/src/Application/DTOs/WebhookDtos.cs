using n8neiritech.Domain.Enums;

namespace n8neiritech.Application.DTOs;

public record InboundWebhookPayload
{
    public string MessageId { get; init; } = string.Empty;
    public string InstanceId { get; init; } = string.Empty;
    public string FromNumber { get; init; } = string.Empty;
    public string ToNumber { get; init; } = string.Empty;
    public string? ContactName { get; init; }
    public string? Text { get; init; }
    public MessageType MessageType { get; init; }
    public DateTime Timestamp { get; init; }
    public string? MediaUrl { get; init; }
    public string? QuotedMessageId { get; init; }
    public MessageStatus Status { get; init; }
    public string? ProviderMetadata { get; init; }
    public string? CorrelationId { get; init; }
}
