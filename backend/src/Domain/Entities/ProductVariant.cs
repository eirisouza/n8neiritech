using n8neiritech.Domain.Common;

namespace n8neiritech.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AttributeName { get; set; }
    public string? AttributeValue { get; set; }
    public decimal? PriceAdjustment { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
    public Product Product { get; set; } = null!;
}
