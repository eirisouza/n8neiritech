using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using n8neiritech.Application.Common;
using n8neiritech.Application.DTOs;
using n8neiritech.Application.Interfaces;
using n8neiritech.Domain.Entities;

namespace n8neiritech.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public CustomersController(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CustomerResponse>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var query = _context.Customers.AsNoTracking().Include(x => x.Addresses).Where(x => x.TenantId == _currentUser.TenantId);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.LastInteractionAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Ok(PagedResult<CustomerResponse>.Create(items.Select(Map), page, pageSize, total));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerResponse>> Get(Guid id, CancellationToken ct)
    {
        var entity = await _context.Customers.AsNoTracking().Include(x => x.Addresses).FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        return entity is null ? NotFound() : Ok(Map(entity));
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> Create(CreateCustomerRequest request, CancellationToken ct)
    {
        var entity = new Customer { TenantId = _currentUser.TenantId, StoreId = request.StoreId, Phone = request.Phone, Name = request.Name, Email = request.Email, BirthDate = request.BirthDate, ConsentMarketing = request.ConsentMarketing, Notes = request.Notes };
        await _context.Customers.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, Map(entity));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerResponse>> Update(Guid id, UpdateCustomerRequest request, CancellationToken ct)
    {
        var entity = await _context.Customers.Include(x => x.Addresses).FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (entity is null) return NotFound();
        entity.Phone = request.Phone; entity.Name = request.Name; entity.Email = request.Email; entity.BirthDate = request.BirthDate; entity.ConsentMarketing = request.ConsentMarketing; entity.Notes = request.Notes;
        await _context.SaveChangesAsync(ct);
        return Ok(Map(entity));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (entity is null) return NotFound();
        _context.Customers.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/conversations")]
    public async Task<IActionResult> GetConversations(Guid id, CancellationToken ct)
        => Ok(await _context.Conversations.AsNoTracking().Where(x => x.CustomerId == id && x.TenantId == _currentUser.TenantId).OrderByDescending(x => x.LastMessageAt).ToListAsync(ct));

    [HttpGet("{id:guid}/orders")]
    public async Task<IActionResult> GetOrders(Guid id, CancellationToken ct)
        => Ok(await _context.Orders.AsNoTracking().Where(x => x.CustomerId == id && x.TenantId == _currentUser.TenantId).OrderByDescending(x => x.CreatedAt).ToListAsync(ct));

    [HttpPut("{id:guid}/block")]
    public async Task<IActionResult> Block(Guid id, BlockCustomerRequest request, CancellationToken ct)
    {
        var entity = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (entity is null) return NotFound();
        entity.IsBlocked = true; entity.BlockReason = request.Reason;
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/unblock")]
    public async Task<IActionResult> Unblock(Guid id, CancellationToken ct)
    {
        var entity = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (entity is null) return NotFound();
        entity.IsBlocked = false; entity.BlockReason = null;
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    private static CustomerResponse Map(Customer x) => new(x.Id, x.StoreId, x.Phone, x.Name, x.Email, x.BirthDate, x.IsBlocked, x.BlockReason, x.ConsentMarketing, x.Notes, x.LastInteractionAt, x.Addresses.Select(a => new CustomerAddressDto(a.Id, a.Label, a.Street, a.Number, a.Complement, a.Neighborhood, a.City, a.State, a.PostalCode, a.IsDefault)).ToArray());
}
