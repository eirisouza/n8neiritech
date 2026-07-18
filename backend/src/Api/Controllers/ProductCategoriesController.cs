using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using n8neiritech.Application.Interfaces;
using n8neiritech.Domain.Entities;

namespace n8neiritech.Api.Controllers;

public record ProductCategoryRequest(Guid StoreId, string Name, string? Description, string? ImageUrl, Guid? ParentId, int SortOrder, bool IsActive);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductCategoriesController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public ProductCategoriesController(IApplicationDbContext context, ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _context.ProductCategories.AsNoTracking().Where(x => x.TenantId == _currentUser.TenantId).OrderBy(x => x.SortOrder).ToListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var category = await _context.ProductCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductCategoryRequest request, CancellationToken ct)
    {
        var category = new ProductCategory { TenantId = _currentUser.TenantId, StoreId = request.StoreId, Name = request.Name, Description = request.Description, ImageUrl = request.ImageUrl, ParentId = request.ParentId, SortOrder = request.SortOrder, IsActive = request.IsActive };
        await _context.ProductCategories.AddAsync(category, ct);
        await _context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = category.Id }, category);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ProductCategoryRequest request, CancellationToken ct)
    {
        var category = await _context.ProductCategories.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (category is null) return NotFound();
        category.Name = request.Name; category.Description = request.Description; category.ImageUrl = request.ImageUrl; category.ParentId = request.ParentId; category.SortOrder = request.SortOrder; category.IsActive = request.IsActive;
        await _context.SaveChangesAsync(ct);
        return Ok(category);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var category = await _context.ProductCategories.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (category is null) return NotFound();
        _context.ProductCategories.Remove(category);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }
}
