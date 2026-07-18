using Microsoft.EntityFrameworkCore;
using n8neiritech.Application.Common;
using n8neiritech.Application.DTOs;
using n8neiritech.Application.Interfaces;

namespace n8neiritech.Application.Services;

public class ProductSearchService
{
    private readonly IApplicationDbContext _context;

    public ProductSearchService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProductListResponse>> SearchAsync(ProductSearchRequest request, Guid tenantId, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = Math.Clamp(request.PageSize <= 0 ? 20 : request.PageSize, 1, 100);
        var query = _context.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Images)
            .Where(x => x.TenantId == tenantId);

        if (request.StoreId.HasValue)
            query = query.Where(x => x.StoreId == request.StoreId.Value);
        if (request.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == request.CategoryId.Value);
        if (request.MinPrice.HasValue)
            query = query.Where(x => (x.PromotionalPrice ?? x.Price) >= request.MinPrice.Value);
        if (request.MaxPrice.HasValue)
            query = query.Where(x => (x.PromotionalPrice ?? x.Price) <= request.MaxPrice.Value);
        if (request.InStock.HasValue)
            query = request.InStock.Value ? query.Where(x => x.Stock > 0) : query.Where(x => x.Stock <= 0);
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var term = request.Q.Trim().ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(term) ||
                x.Sku.ToLower().Contains(term) ||
                (x.Barcode != null && x.Barcode.ToLower().Contains(term)) ||
                (x.Brand != null && x.Brand.ToLower().Contains(term)) ||
                (x.Keywords != null && x.Keywords.ToLower().Contains(term)) ||
                (x.Tags != null && x.Tags.ToLower().Contains(term)) ||
                (x.Category != null && x.Category.Name.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductListResponse(
                x.Id,
                x.Sku,
                x.Name,
                x.Brand,
                x.Price,
                x.PromotionalPrice,
                x.Stock,
                x.IsActive,
                x.Category != null ? x.Category.Name : null,
                x.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault()))
            .ToListAsync(ct);

        return PagedResult<ProductListResponse>.Create(items, page, pageSize, total);
    }
}
