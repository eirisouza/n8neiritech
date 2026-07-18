using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using n8neiritech.Application.DTOs;
using n8neiritech.Application.Interfaces;
using n8neiritech.Domain.Entities;

namespace n8neiritech.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class TenantsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public TenantsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TenantResponse>>> GetAll(CancellationToken ct)
        => Ok(await _context.Tenants.AsNoTracking().Select(x => new TenantResponse(x.Id, x.Name, x.Slug, x.LogoUrl, x.PrimaryColor, x.IsActive, x.Plan, x.PlanExpiresAt)).ToListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TenantResponse>> Get(Guid id, CancellationToken ct)
    {
        var tenant = await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return tenant is null ? NotFound() : Ok(new TenantResponse(tenant.Id, tenant.Name, tenant.Slug, tenant.LogoUrl, tenant.PrimaryColor, tenant.IsActive, tenant.Plan, tenant.PlanExpiresAt));
    }

    [HttpPost]
    public async Task<ActionResult<TenantResponse>> Create([FromBody] CreateTenantRequest request, CancellationToken ct)
    {
        var entity = new Tenant { Name = request.Name, Slug = request.Slug, LogoUrl = request.LogoUrl, PrimaryColor = request.PrimaryColor, Plan = request.Plan, PlanExpiresAt = request.PlanExpiresAt };
        await _context.Tenants.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new TenantResponse(entity.Id, entity.Name, entity.Slug, entity.LogoUrl, entity.PrimaryColor, entity.IsActive, entity.Plan, entity.PlanExpiresAt));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TenantResponse>> Update(Guid id, [FromBody] UpdateTenantRequest request, CancellationToken ct)
    {
        var entity = await _context.Tenants.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound();
        entity.Name = request.Name; entity.Slug = request.Slug; entity.LogoUrl = request.LogoUrl; entity.PrimaryColor = request.PrimaryColor; entity.IsActive = request.IsActive; entity.Plan = request.Plan; entity.PlanExpiresAt = request.PlanExpiresAt;
        await _context.SaveChangesAsync(ct);
        return Ok(new TenantResponse(entity.Id, entity.Name, entity.Slug, entity.LogoUrl, entity.PrimaryColor, entity.IsActive, entity.Plan, entity.PlanExpiresAt));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _context.Tenants.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return NotFound();
        _context.Tenants.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }
}
