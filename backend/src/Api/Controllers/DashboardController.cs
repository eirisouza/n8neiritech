using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using n8neiritech.Application.DTOs;
using n8neiritech.Application.Interfaces;
using n8neiritech.Domain.Enums;

namespace n8neiritech.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public DashboardController(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<DashboardMetrics>> GetMetrics(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var conversationsToday = await _context.Conversations.CountAsync(x => x.TenantId == _currentUser.TenantId && x.CreatedAt >= today, ct);
        var customersToday = await _context.Customers.CountAsync(x => x.TenantId == _currentUser.TenantId && x.CreatedAt >= today, ct);
        var ordersToday = await _context.Orders.CountAsync(x => x.TenantId == _currentUser.TenantId && x.CreatedAt >= today, ct);
        var revenueToday = await _context.Orders.Where(x => x.TenantId == _currentUser.TenantId && x.CreatedAt >= today && x.Status != OrderStatus.Cancelled).SumAsync(x => (decimal?)x.Total, ct) ?? 0m;
        var humanHandoffs = await _context.Conversations.CountAsync(x => x.TenantId == _currentUser.TenantId && (x.Status == ConversationStatus.HumanAttendance || x.Status == ConversationStatus.WaitingAgent), ct);
        var automationErrors = await _context.WebhookEvents.CountAsync(x => x.TenantId == _currentUser.TenantId && !x.Processed, ct);
        var statuses = await _context.WhatsAppInstances.Where(x => x.TenantId == _currentUser.TenantId).Select(x => new InstanceStatusDto(x.Id, x.Name, x.IsConnected, x.ConnectedNumber)).ToListAsync(ct);
        var conversionRate = conversationsToday == 0 ? 0d : Math.Round((double)ordersToday / conversationsToday * 100d, 2);
        return Ok(new DashboardMetrics(conversationsToday, customersToday, ordersToday, revenueToday, conversionRate, humanHandoffs, 0d, automationErrors, statuses));
    }

    [HttpGet("conversations-today")]
    public async Task<IActionResult> ConversationsToday(CancellationToken ct)
        => Ok(await _context.Conversations.AsNoTracking().Where(x => x.TenantId == _currentUser.TenantId && x.CreatedAt >= DateTime.UtcNow.Date).OrderByDescending(x => x.CreatedAt).ToListAsync(ct));

    [HttpGet("orders-today")]
    public async Task<IActionResult> OrdersToday(CancellationToken ct)
        => Ok(await _context.Orders.AsNoTracking().Where(x => x.TenantId == _currentUser.TenantId && x.CreatedAt >= DateTime.UtcNow.Date).OrderByDescending(x => x.CreatedAt).ToListAsync(ct));

    [HttpGet("top-products")]
    public async Task<IActionResult> TopProducts(CancellationToken ct)
        => Ok(await _context.OrderItems.AsNoTracking().GroupBy(x => new { x.ProductId, x.ProductName }).Select(g => new { g.Key.ProductId, g.Key.ProductName, Quantity = g.Sum(x => x.Quantity), Revenue = g.Sum(x => x.Total) }).OrderByDescending(x => x.Quantity).Take(10).ToListAsync(ct));

    [HttpGet("errors")]
    public async Task<IActionResult> Errors(CancellationToken ct)
        => Ok(await _context.WebhookEvents.AsNoTracking().Where(x => x.TenantId == _currentUser.TenantId && !x.Processed).OrderByDescending(x => x.CreatedAt).ToListAsync(ct));
}
