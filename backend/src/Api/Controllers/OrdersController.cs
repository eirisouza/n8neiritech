using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using n8neiritech.Application.Common;
using n8neiritech.Application.DTOs;
using n8neiritech.Application.Interfaces;
using n8neiritech.Application.Services;

namespace n8neiritech.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly OrderService _orderService;

    public OrdersController(IApplicationDbContext context, ICurrentUser currentUser, OrderService orderService)
    {
        _context = context;
        _currentUser = currentUser;
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? customerId = null, [FromQuery] Guid? storeId = null, CancellationToken ct = default)
    {
        var query = _context.Orders.AsNoTracking().Include(x => x.Items).Where(x => x.TenantId == _currentUser.TenantId);
        if (customerId.HasValue) query = query.Where(x => x.CustomerId == customerId.Value);
        if (storeId.HasValue) query = query.Where(x => x.StoreId == storeId.Value);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Ok(PagedResult<OrderResponse>.Create(items.Select(Map), page, pageSize, total));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var entity = await _context.Orders.AsNoTracking().Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        return entity is null ? NotFound() : Ok(Map(entity));
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request, CancellationToken ct)
        => Created(string.Empty, await _orderService.CreateFromCartAsync(_currentUser.TenantId, request, ct));

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<OrderResponse>> UpdateStatus(Guid id, UpdateOrderStatusRequest request, CancellationToken ct)
        => Ok(await _orderService.UpdateStatusAsync(_currentUser.TenantId, id, request, ct));

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancelOrderRequest request, CancellationToken ct)
    {
        await _orderService.CancelAsync(_currentUser.TenantId, id, request.Reason, ct);
        return NoContent();
    }

    private static OrderResponse Map(n8neiritech.Domain.Entities.Order order) => new(order.Id, order.StoreId, order.CustomerId, order.OrderNumber, order.Status, order.Subtotal, order.DeliveryFee, order.Discount, order.Total, order.DeliveryType, order.PaymentMethod, order.DeliveryAddress, order.Notes, order.ConfirmedAt, order.DeliveredAt, order.Items.Select(x => new OrderItemResponse(x.Id, x.ProductId, x.VariantId, x.ProductName, x.VariantName, x.ProductSku, x.Quantity, x.UnitPrice, x.Discount, x.Total)).ToArray());
}
