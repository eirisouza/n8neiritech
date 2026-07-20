using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using n8neiritech.Api.Dtos;
using n8neiritech.Application.DTOs;
using n8neiritech.Application.Interfaces;
using n8neiritech.Application.Services;
using n8neiritech.Domain.Entities;
using n8neiritech.Domain.Enums;
using System.Text;

namespace n8neiritech.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ProductSearchService _productSearchService;

    public ProductsController(IApplicationDbContext context, ICurrentUser currentUser, ProductSearchService productSearchService)
    {
        _context = context;
        _currentUser = currentUser;
        _productSearchService = productSearchService;
    }

    [HttpGet]
    public async Task<ActionResult<object>> Search([FromQuery] string? q, [FromQuery] Guid? categoryId, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] bool? inStock, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? storeId = null, CancellationToken ct = default)
        => Ok(await _productSearchService.SearchAsync(new ProductSearchRequest(q, categoryId, minPrice, maxPrice, inStock, storeId ?? _currentUser.StoreId, page, pageSize), _currentUser.TenantId, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> Get(Guid id, CancellationToken ct)
    {
        var product = await _context.Products.AsNoTracking().Include(x => x.Category).Include(x => x.Images).Include(x => x.Variants).FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        return product is null ? NotFound() : Ok(Map(product));
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var product = new Product { TenantId = _currentUser.TenantId, StoreId = request.StoreId, CategoryId = request.CategoryId, Sku = request.Sku, Name = request.Name, Description = request.Description, ShortDescription = request.ShortDescription, Brand = request.Brand, Unit = request.Unit, Price = request.Price, PromotionalPrice = request.PromotionalPrice, Stock = request.Stock, MinStock = request.MinStock, Weight = request.Weight, Width = request.Width, Height = request.Height, Depth = request.Depth, Barcode = request.Barcode, Tags = request.Tags, Keywords = request.Keywords, SyncSource = request.SyncSource, ExternalId = request.ExternalId };
        await _context.Products.AddAsync(product, ct);
        await _context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = product.Id }, Map(product));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var product = await _context.Products.Include(x => x.Images).Include(x => x.Variants).FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (product is null) return NotFound();
        product.CategoryId = request.CategoryId; product.Sku = request.Sku; product.Name = request.Name; product.Description = request.Description; product.ShortDescription = request.ShortDescription; product.Brand = request.Brand; product.Unit = request.Unit; product.Price = request.Price; product.PromotionalPrice = request.PromotionalPrice; product.Stock = request.Stock; product.MinStock = request.MinStock; product.Weight = request.Weight; product.Width = request.Width; product.Height = request.Height; product.Depth = request.Depth; product.Barcode = request.Barcode; product.IsActive = request.IsActive; product.Tags = request.Tags; product.Keywords = request.Keywords; product.ExternalId = request.ExternalId;
        await _context.SaveChangesAsync(ct);
        return Ok(Map(product));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (product is null) return NotFound();
        _context.Products.Remove(product);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/images")]
    public async Task<IActionResult> AddImage(Guid id, ProductImageRequest request, CancellationToken ct)
    {
        var product = await _context.Products.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (product is null) return NotFound();
        var image = new ProductImage { ProductId = product.Id, Url = request.Url, AltText = request.AltText, SortOrder = request.SortOrder, IsPrimary = request.IsPrimary };
        await _context.ProductImages.AddAsync(image, ct);
        await _context.SaveChangesAsync(ct);
        return Ok(new ProductImageDto(image.Id, image.Url, image.AltText, image.SortOrder, image.IsPrimary));
    }

    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId, CancellationToken ct)
    {
        var image = await _context.ProductImages.Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == id && x.Product.TenantId == _currentUser.TenantId, ct);
        if (image is null) return NotFound();
        _context.ProductImages.Remove(image);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/variants")]
    public async Task<IActionResult> GetVariants(Guid id, CancellationToken ct)
        => Ok(await _context.ProductVariants.AsNoTracking().Where(x => x.ProductId == id).Select(x => new ProductVariantDto(x.Id, x.Sku, x.Name, x.AttributeName, x.AttributeValue, x.PriceAdjustment, x.Stock, x.IsActive)).ToListAsync(ct));

    [HttpPost("{id:guid}/variants")]
    public async Task<IActionResult> AddVariant(Guid id, CreateProductVariantRequest request, CancellationToken ct)
    {
        var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _currentUser.TenantId, ct);
        if (product is null) return NotFound();
        var variant = new ProductVariant { ProductId = id, Sku = request.Sku, Name = request.Name, AttributeName = request.AttributeName, AttributeValue = request.AttributeValue, PriceAdjustment = request.PriceAdjustment, Stock = request.Stock, IsActive = request.IsActive };
        await _context.ProductVariants.AddAsync(variant, ct);
        await _context.SaveChangesAsync(ct);
        return Ok(new ProductVariantDto(variant.Id, variant.Sku, variant.Name, variant.AttributeName, variant.AttributeValue, variant.PriceAdjustment, variant.Stock, variant.IsActive));
    }

    [HttpPut("{id:guid}/variants/{variantId:guid}")]
    public async Task<IActionResult> UpdateVariant(Guid id, Guid variantId, UpdateProductVariantRequest request, CancellationToken ct)
    {
        var variant = await _context.ProductVariants.Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == variantId && x.ProductId == id && x.Product.TenantId == _currentUser.TenantId, ct);
        if (variant is null) return NotFound();
        variant.Sku = request.Sku; variant.Name = request.Name; variant.AttributeName = request.AttributeName; variant.AttributeValue = request.AttributeValue; variant.PriceAdjustment = request.PriceAdjustment; variant.Stock = request.Stock; variant.IsActive = request.IsActive;
        await _context.SaveChangesAsync(ct);
        return Ok(new ProductVariantDto(variant.Id, variant.Sku, variant.Name, variant.AttributeName, variant.AttributeValue, variant.PriceAdjustment, variant.Stock, variant.IsActive));
    }

    [HttpDelete("{id:guid}/variants/{variantId:guid}")]
    public async Task<IActionResult> DeleteVariant(Guid id, Guid variantId, CancellationToken ct)
    {
        var variant = await _context.ProductVariants.Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == variantId && x.ProductId == id && x.Product.TenantId == _currentUser.TenantId, ct);
        if (variant is null) return NotFound();
        _context.ProductVariants.Remove(variant);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromForm] ImportProductRequest request, [FromForm] ProductSyncSource source, [FromForm] Guid storeId, CancellationToken ct)
    {
        if (request.File.Length == 0) return BadRequest("Empty file.");
        using var reader = new StreamReader(request.File.OpenReadStream(), Encoding.UTF8);
        var lines = (await reader.ReadToEndAsync(ct)).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var imported = 0;
        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length < 5) continue;
            await _context.Products.AddAsync(new Product { TenantId = _currentUser.TenantId, StoreId = storeId, Sku = parts[0], Name = parts[1], Description = parts.ElementAtOrDefault(2), Brand = parts.ElementAtOrDefault(3), Price = decimal.TryParse(parts.ElementAtOrDefault(4), out var price) ? price : 0m, Stock = parts.Length > 5 && int.TryParse(parts[5], out var stock) ? stock : 0, MinStock = 0, SyncSource = source }, ct);
            imported++;
        }
        await _context.SaveChangesAsync(ct);
        return Ok(new { imported });
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync(CancellationToken ct)
    {
        var products = await _context.Products.Where(x => x.TenantId == _currentUser.TenantId).ToListAsync(ct);
        foreach (var product in products) product.LastSyncAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Ok(new { synced = products.Count });
    }

    [HttpPatch("batch-update")]
    public async Task<IActionResult> BatchUpdate(ProductBatchUpdateRequest request, CancellationToken ct)
    {
        var products = await _context.Products.Where(x => request.ProductIds.Contains(x.Id) && x.TenantId == _currentUser.TenantId).ToListAsync(ct);
        foreach (var product in products)
        {
            if (request.IsActive.HasValue) product.IsActive = request.IsActive.Value;
            if (request.PriceAdjustmentPercent.HasValue) product.Price += product.Price * (request.PriceAdjustmentPercent.Value / 100m);
            if (request.StockDelta.HasValue) product.Stock += request.StockDelta.Value;
        }
        await _context.SaveChangesAsync(ct);
        return Ok(new { updated = products.Count });
    }

    private static ProductResponse Map(Product product) => new(product.Id, product.StoreId, product.CategoryId, product.Sku, product.Name, product.Description, product.ShortDescription, product.Brand, product.Unit, product.Price, product.PromotionalPrice, product.Stock, product.MinStock, product.Barcode, product.IsActive, product.Tags, product.Keywords, product.SyncSource, product.LastSyncAt, product.ExternalId, product.Category?.Name, product.Images.Select(i => new ProductImageDto(i.Id, i.Url, i.AltText, i.SortOrder, i.IsPrimary)).ToArray(), product.Variants.Select(v => new ProductVariantDto(v.Id, v.Sku, v.Name, v.AttributeName, v.AttributeValue, v.PriceAdjustment, v.Stock, v.IsActive)).ToArray());
}
