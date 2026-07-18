using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using n8neiritech.Application.DTOs;
using n8neiritech.Application.Interfaces;
using n8neiritech.Domain.Entities;

namespace n8neiritech.Application.Services;

public class WebhookService
{
    private readonly IApplicationDbContext _context;
    private readonly ConversationService _conversationService;

    public WebhookService(IApplicationDbContext context, ConversationService conversationService)
    {
        _context = context;
        _conversationService = conversationService;
    }

    public async Task<ConversationDetailResponse?> ProcessInboundAsync(InboundWebhookPayload payload, CancellationToken ct = default)
    {
        var existing = await _context.WebhookEvents.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ExternalEventId == payload.MessageId && x.EventType == "inbound.message", ct);
        if (existing is not null)
        {
            var conversation = await _context.Conversations.AsNoTracking()
                .Where(x => x.WhatsAppInstanceId != null)
                .OrderByDescending(x => x.LastMessageAt)
                .FirstOrDefaultAsync(ct);
            return conversation is null ? null : await _conversationService.GetDetailAsync(conversation.Id, ct);
        }

        var evt = await RegisterEventAsync(payload, ct);
        try
        {
            var result = await _conversationService.ProcessInboundMessageAsync(payload, ct);
            await MarkProcessedAsync(evt.Id, null, ct);
            return result;
        }
        catch (Exception ex)
        {
            await MarkProcessedAsync(evt.Id, ex.Message, ct);
            throw;
        }
    }

    public async Task<WebhookEvent> RegisterEventAsync(InboundWebhookPayload payload, CancellationToken ct = default)
    {
        var instance = await _context.WhatsAppInstances.FirstOrDefaultAsync(
            x => x.InstanceName == payload.InstanceId || x.Id.ToString() == payload.InstanceId,
            ct) ?? throw new InvalidOperationException("WhatsApp instance not found.");

        var evt = new WebhookEvent
        {
            TenantId = instance.TenantId,
            StoreId = instance.StoreId,
            WhatsAppInstanceId = instance.Id,
            ExternalEventId = payload.MessageId,
            EventType = "inbound.message",
            RawPayload = JsonSerializer.Serialize(payload),
            CorrelationId = payload.CorrelationId,
            Processed = false
        };
        await _context.WebhookEvents.AddAsync(evt, ct);
        await _context.SaveChangesAsync(ct);
        return evt;
    }

    public async Task MarkProcessedAsync(Guid eventId, string? processingError, CancellationToken ct = default)
    {
        var evt = await _context.WebhookEvents.FirstOrDefaultAsync(x => x.Id == eventId, ct)
            ?? throw new KeyNotFoundException("Webhook event not found.");
        evt.Processed = string.IsNullOrWhiteSpace(processingError);
        evt.ProcessedAt = DateTime.UtcNow;
        evt.ProcessingError = processingError;
        if (!evt.Processed)
        {
            evt.RetryCount += 1;
        }

        await _context.SaveChangesAsync(ct);
    }
}
