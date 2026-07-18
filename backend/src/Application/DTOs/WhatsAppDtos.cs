using n8neiritech.Domain.Enums;

namespace n8neiritech.Application.DTOs;

public record WhatsAppInstanceRequest(Guid StoreId, string Name, string InstanceName, WhatsAppProviderType ProviderType, string? BaseUrl, string? ApiToken, string? WebhookSecret);
public record WhatsAppInstanceResponse(Guid Id, Guid StoreId, string Name, string InstanceName, WhatsAppProviderType ProviderType, string? ConnectedNumber, bool IsConnected, bool IsActive, string? LastError);
public record WhatsAppSendRequest(Guid InstanceId, string To, string Text, MessageType Type = MessageType.Text, string? MediaUrl = null);
