using n8neiritech.Domain.Common;
using n8neiritech.Domain.Enums;

namespace n8neiritech.Domain.Entities;

public class Product : TenantEntity
{
    public Guid StoreId { get; set; }
    public Guid? CategoryId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string? Brand { get; set; }
    public string? Unit { get; set; } = "un";
    public decimal Price { get; set; }
    public decimal? PromotionalPrice { get; set; }
    public int Stock { get; set; }
    public int MinStock { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public decimal? Depth { get; set; }
    public string? Barcode { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Tags { get; set; }
    public string? Keywords { get; set; }
    public ProductSyncSource SyncSource { get; set; } = ProductSyncSource.Manual;
    public DateTime? LastSyncAt { get; set; }
    public string? ExternalId { get; set; }
    public Store Store { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public ProductCategory? Category { get; set; }
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}
