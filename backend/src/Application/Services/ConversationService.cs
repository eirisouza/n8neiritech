using Microsoft.EntityFrameworkCore;
using n8neiritech.Application.DTOs;
using n8neiritech.Application.Interfaces;
using n8neiritech.Domain.Entities;
using n8neiritech.Domain.Enums;

namespace n8neiritech.Application.Services;

public class ConversationService
{
    private readonly IApplicationDbContext _context;
    private readonly IAiProvider _aiProvider;
    private readonly IWhatsAppProvider _whatsAppProvider;

    public ConversationService(IApplicationDbContext context, IAiProvider aiProvider, IWhatsAppProvider whatsAppProvider)
    {
        _context = context;
        _aiProvider = aiProvider;
        _whatsAppProvider = whatsAppProvider;
    }

    public async Task<ConversationDetailResponse> ProcessInboundMessageAsync(InboundWebhookPayload payload, CancellationToken ct = default)
    {
        var instance = await _context.WhatsAppInstances.FirstOrDefaultAsync(
            x => x.InstanceName == payload.InstanceId || x.Id.ToString() == payload.InstanceId,
            ct) ?? throw new InvalidOperationException("WhatsApp instance not found.");

        var customer = await _context.Customers
            .Include(x => x.Addresses)
            .FirstOrDefaultAsync(x => x.TenantId == instance.TenantId && x.StoreId == instance.StoreId && x.Phone == payload.FromNumber, ct);

        if (customer is null)
        {
            customer = new Customer
            {
                TenantId = instance.TenantId,
                StoreId = instance.StoreId,
                Phone = payload.FromNumber,
                Name = payload.ContactName,
                LastInteractionAt = payload.Timestamp
            };
            await _context.Customers.AddAsync(customer, ct);
        }
        else
        {
            customer.Name ??= payload.ContactName;
            customer.LastInteractionAt = payload.Timestamp;
        }

        var conversation = await _context.Conversations
            .Include(x => x.Customer)
            .Include(x => x.Messages.OrderBy(m => m.CreatedAt))
            .Include(x => x.AssignedAgent)
            .FirstOrDefaultAsync(x => x.TenantId == instance.TenantId && x.StoreId == instance.StoreId && x.CustomerId == customer.Id && x.Status != ConversationStatus.Finished && x.Status != ConversationStatus.Cancelled, ct);

        if (conversation is null)
        {
            conversation = new Conversation
            {
                TenantId = instance.TenantId,
                StoreId = instance.StoreId,
                Customer = customer,
                CustomerId = customer.Id,
                WhatsAppInstanceId = instance.Id,
                Status = ConversationStatus.New,
                LastMessageAt = payload.Timestamp,
                UnreadCount = 0
            };
            await _context.Conversations.AddAsync(conversation, ct);
        }

        var existingMessage = await _context.ConversationMessages.FirstOrDefaultAsync(x => x.ExternalMessageId == payload.MessageId, ct);
        if (existingMessage is null)
        {
            await _context.ConversationMessages.AddAsync(new ConversationMessage
            {
                Conversation = conversation,
                ConversationId = conversation.Id,
                ExternalMessageId = payload.MessageId,
                Direction = MessageDirection.Inbound,
                Type = payload.MessageType,
                Text = payload.Text,
                MediaUrl = payload.MediaUrl,
                Status = payload.Status,
                CorrelationId = payload.CorrelationId,
                RawPayload = payload.ProviderMetadata
            }, ct);
        }

        conversation.LastMessageAt = payload.Timestamp;
        conversation.UnreadCount += 1;

        if (!conversation.AutomationPaused && payload.MessageType == MessageType.Text && !string.IsNullOrWhiteSpace(payload.Text))
        {
            var response = await BuildAutomatedResponseAsync(conversation, customer, instance, payload.Text!, ct);
            if (!string.IsNullOrWhiteSpace(response))
            {
                var sendResult = await _whatsAppProvider.SendTextAsync(instance.InstanceName, customer.Phone, response, payload.MessageId, ct);
                await _context.ConversationMessages.AddAsync(new ConversationMessage
                {
                    Conversation = conversation,
                    ConversationId = conversation.Id,
                    Direction = MessageDirection.Outbound,
                    Type = MessageType.Text,
                    Text = response,
                    Status = sendResult.Success ? MessageStatus.Sent : MessageStatus.Failed,
                    ExternalMessageId = sendResult.MessageId,
                    CorrelationId = payload.CorrelationId,
                    RawPayload = sendResult.Error
                }, ct);
            }
        }

        await _context.SaveChangesAsync(ct);
        return await GetDetailAsync(conversation.Id, ct) ?? throw new InvalidOperationException("Conversation could not be loaded.");
    }

    public async Task AssignAgentAsync(Guid conversationId, Guid agentId, CancellationToken ct = default)
    {
        var conversation = await GetConversationAsync(conversationId, ct);
        conversation.AssignedAgentId = agentId;
        conversation.Status = ConversationStatus.HumanAttendance;
        await _context.SaveChangesAsync(ct);
    }

    public Task TransferAsync(Guid conversationId, Guid agentId, CancellationToken ct = default) => AssignAgentAsync(conversationId, agentId, ct);

    public async Task CloseAsync(Guid conversationId, CancellationToken ct = default)
    {
        var conversation = await GetConversationAsync(conversationId, ct);
        conversation.Status = ConversationStatus.Finished;
        conversation.UnreadCount = 0;
        await _context.SaveChangesAsync(ct);
    }

    public async Task ReopenAsync(Guid conversationId, CancellationToken ct = default)
    {
        var conversation = await GetConversationAsync(conversationId, ct);
        conversation.Status = ConversationStatus.MainMenu;
        await _context.SaveChangesAsync(ct);
    }

    public async Task PauseBotAsync(Guid conversationId, DateTime? until = null, CancellationToken ct = default)
    {
        var conversation = await GetConversationAsync(conversationId, ct);
        conversation.AutomationPaused = true;
        conversation.AutomationPausedUntil = until;
        await _context.SaveChangesAsync(ct);
    }

    public async Task ResumeBotAsync(Guid conversationId, CancellationToken ct = default)
    {
        var conversation = await GetConversationAsync(conversationId, ct);
        conversation.AutomationPaused = false;
        conversation.AutomationPausedUntil = null;
        await _context.SaveChangesAsync(ct);
    }

    public async Task<string?> GetSessionDataAsync(Guid conversationId, CancellationToken ct = default)
    {
        var conversation = await GetConversationAsync(conversationId, ct);
        return conversation.SessionData;
    }

    public async Task UpdateSessionDataAsync(Guid conversationId, string sessionData, CancellationToken ct = default)
    {
        var conversation = await GetConversationAsync(conversationId, ct);
        conversation.SessionData = sessionData;
        await _context.SaveChangesAsync(ct);
    }

    public async Task<ConversationDetailResponse?> GetDetailAsync(Guid conversationId, CancellationToken ct = default)
    {
        var conversation = await _context.Conversations.AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.AssignedAgent)
            .Include(x => x.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(x => x.Id == conversationId, ct);

        return conversation is null ? null : MapDetail(conversation);
    }

    private async Task<Conversation> GetConversationAsync(Guid conversationId, CancellationToken ct)
        => await _context.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId, ct)
           ?? throw new KeyNotFoundException("Conversation not found.");

    private async Task<string> BuildAutomatedResponseAsync(Conversation conversation, Customer customer, WhatsAppInstance instance, string message, CancellationToken ct)
    {
        var normalized = message.Trim().ToLowerInvariant();
        if (normalized.Contains("atendente") || normalized.Contains("humano"))
        {
            conversation.Status = ConversationStatus.WaitingAgent;
            return "Certo! Vou encaminhar sua conversa para um atendente humano.";
        }

        if (normalized.Contains("pedido") || normalized.Contains("comprar"))
        {
            conversation.Status = ConversationStatus.BuildingCart;
        }
        else
        {
            conversation.Status = ConversationStatus.MainMenu;
        }

        var intent = await _aiProvider.DetectIntentAsync(message, conversation.SessionData, ct);
        var prompt = $"Cliente: {customer.Name ?? customer.Phone}; intenção={intent.Intent}; loja={instance.Name}";
        var response = await _aiProvider.GenerateResponseAsync(prompt, message, ct);
        return string.IsNullOrWhiteSpace(response)
            ? "Recebi sua mensagem e já vou te ajudar com opções, preços e pedido."
            : response;
    }

    private static ConversationDetailResponse MapDetail(Conversation conversation)
        => new(
            conversation.Id,
            conversation.StoreId,
            conversation.CustomerId,
            conversation.Customer.Name,
            conversation.Customer.Phone,
            conversation.Status,
            conversation.AssignedAgentId,
            conversation.AssignedAgent?.Name,
            conversation.AutomationPaused,
            conversation.SessionData,
            conversation.Notes,
            conversation.Messages
                .OrderBy(x => x.CreatedAt)
                .Select(x => new MessageDto(x.Id, x.ConversationId, x.Direction, x.Type, x.Text, x.MediaUrl, x.Status, x.IsInternal, x.SentByUserId, x.CreatedAt))
                .ToArray());
}
