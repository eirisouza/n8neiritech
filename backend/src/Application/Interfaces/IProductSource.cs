using n8neiritech.Domain.Enums;

namespace n8neiritech.Application.Interfaces;

public interface IProductSource
{
    Task<IEnumerable<ProductImportDto>> ImportAsync(ProductSyncSource source, string? config = null, CancellationToken ct = default);
}

public record ProductImportDto(string Sku, string Name, string? Description, string? Category, string? Brand, decimal Price, decimal? PromotionalPrice, int Stock, string? Barcode, string? Tags);
