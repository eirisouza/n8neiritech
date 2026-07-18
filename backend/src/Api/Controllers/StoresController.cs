using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using n8neiritech.Application.DTOs;
using n8neiritech.Application.Interfaces;
using n8neiritech.Domain.Entities;

namespace n8neiritech.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StoresController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public StoresController(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StoreResponse>>> GetAll(CancellationToken ct)
    {
        var items = await _context.Stores.AsNoTracking().Where(x => x.TenantId == _currentUser.TenantId)
            .Select(x => new StoreResponse(x.Id, x.TenantId, x.Name, x.Description, x.LogoUrl, x.Phone, x.Email, x.Website, x.AddressLine, x.City, x.State, x.PostalCode, x.Country, x.IsActive, x.BusinessType))
            .ToListAsync(ct);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StoreResponse>> Get(Guid id, CancellationToken ct)
    {
        var x = await _context.Stores.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        return x is null ? NotFound() : Ok(new StoreResponse(x.Id, x.TenantId, x.Name, x.Description, x.LogoUrl, x.Phone, x.Email, x.Website, x.AddressLine, x.City, x.State, x.PostalCode, x.Country, x.IsActive, x.BusinessType));
    }

    [HttpPost]
    public async Task<ActionResult<StoreResponse>> Create(CreateStoreRequest request, CancellationToken ct)
    {
        var entity = new Store { TenantId = _currentUser.TenantId == Guid.Empty ? request.TenantId : _currentUser.TenantId, Name = request.Name, Description = request.Description, LogoUrl = request.LogoUrl, Phone = request.Phone, Email = request.Email, Website = request.Website, AddressLine = request.AddressLine, City = request.City, State = request.State, PostalCode = request.PostalCode, Country = request.Country, BusinessType = request.BusinessType };
        await _context.Stores.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new StoreResponse(entity.Id, entity.TenantId, entity.Name, entity.Description, entity.LogoUrl, entity.Phone, entity.Email, entity.Website, entity.AddressLine, entity.City, entity.State, entity.PostalCode, entity.Country, entity.IsActive, entity.BusinessType));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StoreResponse>> Update(Guid id, UpdateStoreRequest request, CancellationToken ct)
    {
        var entity = await _context.Stores.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (entity is null) return NotFound();
        entity.Name = request.Name; entity.Description = request.Description; entity.LogoUrl = request.LogoUrl; entity.Phone = request.Phone; entity.Email = request.Email; entity.Website = request.Website; entity.AddressLine = request.AddressLine; entity.City = request.City; entity.State = request.State; entity.PostalCode = request.PostalCode; entity.Country = request.Country; entity.IsActive = request.IsActive; entity.BusinessType = request.BusinessType;
        await _context.SaveChangesAsync(ct);
        return Ok(new StoreResponse(entity.Id, entity.TenantId, entity.Name, entity.Description, entity.LogoUrl, entity.Phone, entity.Email, entity.Website, entity.AddressLine, entity.City, entity.State, entity.PostalCode, entity.Country, entity.IsActive, entity.BusinessType));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _context.Stores.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (entity is null) return NotFound();
        _context.Stores.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/hours")]
    public async Task<ActionResult<IEnumerable<BusinessHourDto>>> GetHours(Guid id, CancellationToken ct)
        => Ok(await _context.BusinessHours.AsNoTracking().Where(x => x.StoreId == id && x.TenantId == _currentUser.TenantId).OrderBy(x => x.DayOfWeek).Select(x => new BusinessHourDto(x.Id, x.StoreId, x.DayOfWeek, x.IsOpen, x.OpenTime, x.CloseTime)).ToListAsync(ct));

    [HttpPut("{id:guid}/hours")]
    public async Task<IActionResult> UpdateHours(Guid id, [FromBody] IEnumerable<BusinessHourDto> hours, CancellationToken ct)
    {
        var existing = await _context.BusinessHours.Where(x => x.StoreId == id && x.TenantId == _currentUser.TenantId).ToListAsync(ct);
        _context.BusinessHours.RemoveRange(existing);
        await _context.SaveChangesAsync(ct);
        foreach (var hour in hours)
        {
            await _context.BusinessHours.AddAsync(new BusinessHour { TenantId = _currentUser.TenantId, StoreId = id, DayOfWeek = hour.DayOfWeek, IsOpen = hour.IsOpen, OpenTime = hour.OpenTime, CloseTime = hour.CloseTime }, ct);
        }
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }
}
