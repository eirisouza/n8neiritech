using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using n8neiritech.Application.DTOs;
using n8neiritech.Application.Interfaces;
using n8neiritech.Application.Services;
using n8neiritech.Domain.Entities;
using n8neiritech.Domain.Enums;
using n8neiritech.Infrastructure.Options;
using n8neiritech.Infrastructure.WhatsApp;

namespace n8neiritech.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WhatsAppController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IWhatsAppProvider _defaultProvider;
    private readonly WhatsAppProviderFactory _factory;
    private readonly WebhookService _webhookService;
    private readonly WhatsAppOptions _options;

    public WhatsAppController(IApplicationDbContext context, ICurrentUser currentUser, IWhatsAppProvider defaultProvider, WhatsAppProviderFactory factory, WebhookService webhookService, IOptions<WhatsAppOptions> options)
    {
        _context = context;
        _currentUser = currentUser;
        _defaultProvider = defaultProvider;
        _factory = factory;
        _webhookService = webhookService;
        _options = options.Value;
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] InboundWebhookPayload payload, CancellationToken ct)
    {
        var secret = Request.Headers["X-Webhook-Secret"].FirstOrDefault() ?? Request.Headers["X-Hub-Signature"].FirstOrDefault();
        if (!string.Equals(secret, _options.WebhookSecret, StringComparison.Ordinal)) return Unauthorized();
        var result = await _webhookService.ProcessInboundAsync(payload, ct);
        return result is null ? Ok(new { message = "duplicate" }) : Ok(result);
    }

    [Authorize]
    [HttpGet("instances")]
    public async Task<IActionResult> GetInstances(CancellationToken ct)
        => Ok(await _context.WhatsAppInstances.AsNoTracking().Where(x => x.TenantId == _currentUser.TenantId).Select(x => new WhatsAppInstanceResponse(x.Id, x.StoreId, x.Name, x.InstanceName, x.ProviderType, x.ConnectedNumber, x.IsConnected, x.IsActive, x.LastError)).ToListAsync(ct));

    [Authorize]
    [HttpPost("instances")]
    public async Task<IActionResult> CreateInstance(WhatsAppInstanceRequest request, CancellationToken ct)
    {
        var entity = new WhatsAppInstance { TenantId = _currentUser.TenantId, StoreId = request.StoreId, Name = request.Name, InstanceName = request.InstanceName, ProviderType = request.ProviderType, BaseUrl = request.BaseUrl, ApiToken = request.ApiToken, WebhookSecret = request.WebhookSecret };
        await _context.WhatsAppInstances.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return Created(string.Empty, new WhatsAppInstanceResponse(entity.Id, entity.StoreId, entity.Name, entity.InstanceName, entity.ProviderType, entity.ConnectedNumber, entity.IsConnected, entity.IsActive, entity.LastError));
    }

    [Authorize]
    [HttpGet("instances/{id:guid}/status")]
    public async Task<IActionResult> GetStatus(Guid id, CancellationToken ct)
    {
        var instance = await _context.WhatsAppInstances.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (instance is null) return NotFound();
        var provider = _factory.Create(instance.ProviderType);
        var status = await provider.GetStatusAsync(instance.InstanceName, ct);
        instance.IsConnected = status.Connected; instance.ConnectedNumber = status.Phone; instance.LastStatusCheck = DateTime.UtcNow; instance.LastError = status.Connected ? null : status.Status;
        await _context.SaveChangesAsync(ct);
        return Ok(status);
    }

    [Authorize]
    [HttpGet("instances/{id:guid}/qrcode")]
    public async Task<IActionResult> GetQrCode(Guid id, CancellationToken ct)
    {
        var instance = await _context.WhatsAppInstances.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (instance is null) return NotFound();
        var provider = _factory.Create(instance.ProviderType);
        return Ok(await provider.GetQrCodeAsync(instance.InstanceName, ct));
    }

    [Authorize]
    [HttpPost("instances/{id:guid}/restart")]
    public async Task<IActionResult> Restart(Guid id, CancellationToken ct)
    {
        var instance = await _context.WhatsAppInstances.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (instance is null) return NotFound();
        instance.LastStatusCheck = DateTime.UtcNow; instance.LastError = null;
        await _context.SaveChangesAsync(ct);
        return Ok(new { restarted = true });
    }

    [Authorize]
    [HttpPost("instances/{id:guid}/test")]
    public async Task<IActionResult> Test(Guid id, CancellationToken ct)
    {
        var instance = await _context.WhatsAppInstances.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (instance is null) return NotFound();
        var provider = _factory.Create(instance.ProviderType);
        return Ok(await provider.SendTextAsync(instance.InstanceName, instance.ConnectedNumber ?? "+5511999999999", "Mensagem de teste da plataforma.", null, ct));
    }

    [Authorize]
    [HttpPost("send")]
    public async Task<IActionResult> Send(WhatsAppSendRequest request, CancellationToken ct)
    {
        var instance = await _context.WhatsAppInstances.FirstOrDefaultAsync(x => x.Id == request.InstanceId && x.TenantId == _currentUser.TenantId, ct);
        if (instance is null) return NotFound();
        var provider = _factory.Create(instance.ProviderType);
        var result = request.Type switch
        {
            MessageType.Image => await provider.SendImageAsync(instance.InstanceName, request.To, request.MediaUrl!, request.Text, ct),
            MessageType.Document => await provider.SendDocumentAsync(instance.InstanceName, request.To, request.MediaUrl!, null, ct),
            MessageType.Audio => await provider.SendAudioAsync(instance.InstanceName, request.To, request.MediaUrl!, ct),
            _ => await provider.SendTextAsync(instance.InstanceName, request.To, request.Text, null, ct)
        };
        return Ok(result);
    }
}
