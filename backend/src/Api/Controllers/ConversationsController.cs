using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using n8neiritech.Application.DTOs;
using n8neiritech.Application.Interfaces;
using n8neiritech.Application.Services;
using n8neiritech.Domain.Entities;
using n8neiritech.Domain.Enums;

namespace n8neiritech.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ConversationService _conversationService;
    private readonly IWhatsAppProvider _whatsAppProvider;

    public ConversationsController(IApplicationDbContext context, ICurrentUser currentUser, ConversationService conversationService, IWhatsAppProvider whatsAppProvider)
    {
        _context = context;
        _currentUser = currentUser;
        _conversationService = conversationService;
        _whatsAppProvider = whatsAppProvider;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ConversationStatus? status, [FromQuery] Guid? assignedTo, [FromQuery] Guid? storeId, CancellationToken ct)
    {
        var query = _context.Conversations.AsNoTracking().Include(x => x.Customer).Include(x => x.AssignedAgent).Where(x => x.TenantId == _currentUser.TenantId);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (assignedTo.HasValue) query = query.Where(x => x.AssignedAgentId == assignedTo.Value);
        if (storeId.HasValue) query = query.Where(x => x.StoreId == storeId.Value);
        return Ok(await query.OrderByDescending(x => x.LastMessageAt).Select(x => new ConversationListResponse(x.Id, x.StoreId, x.CustomerId, x.Customer.Name, x.Customer.Phone, x.Status, x.AssignedAgentId, x.AssignedAgent != null ? x.AssignedAgent.Name : null, x.LastMessageAt, x.UnreadCount, x.AutomationPaused)).ToListAsync(ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var detail = await _conversationService.GetDetailAsync(id, ct);
        return detail is null ? NotFound() : Ok(detail);
    }

    [HttpGet("{id:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid id, CancellationToken ct)
        => Ok(await _context.ConversationMessages.AsNoTracking().Where(x => x.ConversationId == id).OrderBy(x => x.CreatedAt).Select(x => new MessageDto(x.Id, x.ConversationId, x.Direction, x.Type, x.Text, x.MediaUrl, x.Status, x.IsInternal, x.SentByUserId, x.CreatedAt)).ToListAsync(ct));

    [HttpPost("{id:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid id, SendMessageRequest request, CancellationToken ct)
    {
        var conversation = await _context.Conversations.Include(x => x.Customer).Include(x => x.WhatsAppInstance).FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (conversation is null || conversation.WhatsAppInstance is null) return NotFound();
        SendMessageResult result = request.Type switch
        {
            MessageType.Image => await _whatsAppProvider.SendImageAsync(conversation.WhatsAppInstance.InstanceName, conversation.Customer.Phone, request.MediaUrl!, request.Text, ct),
            MessageType.Document => await _whatsAppProvider.SendDocumentAsync(conversation.WhatsAppInstance.InstanceName, conversation.Customer.Phone, request.MediaUrl!, null, ct),
            MessageType.Audio => await _whatsAppProvider.SendAudioAsync(conversation.WhatsAppInstance.InstanceName, conversation.Customer.Phone, request.MediaUrl!, ct),
            _ => await _whatsAppProvider.SendTextAsync(conversation.WhatsAppInstance.InstanceName, conversation.Customer.Phone, request.Text, request.QuotedMessageId, ct)
        };
        var message = new ConversationMessage { ConversationId = id, Direction = MessageDirection.Outbound, Type = request.Type, Text = request.Text, MediaUrl = request.MediaUrl, Status = result.Success ? MessageStatus.Sent : MessageStatus.Failed, IsInternal = request.IsInternal, SentByUserId = _currentUser.Id.ToString(), ExternalMessageId = result.MessageId, RawPayload = result.Error };
        await _context.ConversationMessages.AddAsync(message, ct);
        await _context.SaveChangesAsync(ct);
        return Ok(new MessageDto(message.Id, message.ConversationId, message.Direction, message.Type, message.Text, message.MediaUrl, message.Status, message.IsInternal, message.SentByUserId, message.CreatedAt));
    }

    [HttpPut("{id:guid}/assign")]
    public async Task<IActionResult> Assign(Guid id, AssignConversationRequest request, CancellationToken ct) { await _conversationService.AssignAgentAsync(id, request.AgentId, ct); return NoContent(); }
    [HttpPut("{id:guid}/transfer")]
    public async Task<IActionResult> Transfer(Guid id, TransferConversationRequest request, CancellationToken ct) { await _conversationService.TransferAsync(id, request.AgentId, ct); return NoContent(); }
    [HttpPut("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken ct) { await _conversationService.CloseAsync(id, ct); return NoContent(); }
    [HttpPut("{id:guid}/reopen")]
    public async Task<IActionResult> Reopen(Guid id, CancellationToken ct) { await _conversationService.ReopenAsync(id, ct); return NoContent(); }
    [HttpPut("{id:guid}/pause-bot")]
    public async Task<IActionResult> Pause(Guid id, CancellationToken ct) { await _conversationService.PauseBotAsync(id, DateTime.UtcNow.AddHours(1), ct); return NoContent(); }
    [HttpPut("{id:guid}/resume-bot")]
    public async Task<IActionResult> Resume(Guid id, CancellationToken ct) { await _conversationService.ResumeBotAsync(id, ct); return NoContent(); }
}
