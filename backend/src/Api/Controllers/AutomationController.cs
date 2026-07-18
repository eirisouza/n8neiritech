using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using n8neiritech.Application.DTOs;
using n8neiritech.Application.Interfaces;
using n8neiritech.Application.Services;

namespace n8neiritech.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AutomationController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly WebhookService _webhookService;
    private readonly ConversationService _conversationService;
    private readonly IApplicationDbContext _context;

    public AutomationController(IConfiguration configuration, WebhookService webhookService, ConversationService conversationService, IApplicationDbContext context)
    {
        _configuration = configuration;
        _webhookService = webhookService;
        _conversationService = conversationService;
        _context = context;
    }

    [HttpPost("process-message")]
    public async Task<IActionResult> ProcessMessage(InboundWebhookPayload payload, CancellationToken ct)
    {
        if (!IsAuthorized()) return Unauthorized();
        return Ok(await _webhookService.ProcessInboundAsync(payload, ct));
    }

    [HttpGet("session/{conversationId:guid}")]
    public async Task<IActionResult> GetSession(Guid conversationId, CancellationToken ct)
    {
        if (!IsAuthorized()) return Unauthorized();
        return Ok(new { sessionData = await _conversationService.GetSessionDataAsync(conversationId, ct) });
    }

    [HttpPut("session/{conversationId:guid}")]
    public async Task<IActionResult> UpdateSession(Guid conversationId, SessionDataRequest request, CancellationToken ct)
    {
        if (!IsAuthorized()) return Unauthorized();
        await _conversationService.UpdateSessionDataAsync(conversationId, request.SessionData, ct);
        return NoContent();
    }

    [HttpPost("register-event")]
    public async Task<IActionResult> RegisterEvent(InboundWebhookPayload payload, CancellationToken ct)
    {
        if (!IsAuthorized()) return Unauthorized();
        return Ok(await _webhookService.RegisterEventAsync(payload, ct));
    }

    [HttpPost("reprocess-event/{eventId:guid}")]
    public async Task<IActionResult> Reprocess(Guid eventId, CancellationToken ct)
    {
        if (!IsAuthorized()) return Unauthorized();
        var evt = await _context.WebhookEvents.FirstOrDefaultAsync(x => x.Id == eventId, ct);
        if (evt is null) return NotFound();
        var payload = JsonSerializer.Deserialize<InboundWebhookPayload>(evt.RawPayload);
        if (payload is null) return BadRequest();
        return Ok(await _webhookService.ProcessInboundAsync(payload, ct));
    }

    private bool IsAuthorized() => string.Equals(Request.Headers["X-API-Key"].FirstOrDefault(), _configuration["InternalApiKey"], StringComparison.Ordinal);
}
