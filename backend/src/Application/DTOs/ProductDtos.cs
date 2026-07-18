using n8neiritech.Domain.Enums;

namespace n8neiritech.Application.DTOs;

public record CreateProductRequest(
    Guid StoreId,
    Guid? CategoryId,
    string Sku,
    string Name,
    string? Description,
    string? ShortDescription,
    string? Brand,
    string? Unit,
    decimal Price,
    decimal? PromotionalPrice,
    int Stock,
    int MinStock,
    decimal? Weight,
    decimal? Width,
    decimal? Height,
    decimal? Depth,
    string? Barcode,
    string? Tags,
    string? Keywords,
    ProductSyncSource SyncSource,
    string? ExternalId);

public record UpdateProductRequest(
    Guid? CategoryId,
    string Sku,
    string Name,
    string? Description,
    string? ShortDescription,
    string? Brand,
    string? Unit,
    decimal Price,
    decimal? PromotionalPrice,
    int Stock,
    int MinStock,
    decimal? Weight,
    decimal? Width,
    decimal? Height,
    decimal? Depth,
    string? Barcode,
    bool IsActive,
    string? Tags,
    string? Keywords,
    string? ExternalId);

public record ProductImageRequest(string Url, string? AltText, int SortOrder, bool IsPrimary);
public record ProductImageDto(Guid Id, string Url, string? AltText, int SortOrder, bool IsPrimary);
public record CreateProductVariantRequest(string Sku, string Name, string? AttributeName, string? AttributeValue, decimal? PriceAdjustment, int Stock, bool IsActive);
public record UpdateProductVariantRequest(string Sku, string Name, string? AttributeName, string? AttributeValue, decimal? PriceAdjustment, int Stock, bool IsActive);
public record ProductVariantDto(Guid Id, string Sku, string Name, string? AttributeName, string? AttributeValue, decimal? PriceAdjustment, int Stock, bool IsActive);
public record ProductResponse(Guid Id, Guid StoreId, Guid? CategoryId, string Sku, string Name, string? Description, string? ShortDescription, string? Brand, string? Unit, decimal Price, decimal? PromotionalPrice, int Stock, int MinStock, string? Barcode, bool IsActive, string? Tags, string? Keywords, ProductSyncSource SyncSource, DateTime? LastSyncAt, string? ExternalId, string? CategoryName, IReadOnlyCollection<ProductImageDto> Images, IReadOnlyCollection<ProductVariantDto> Variants);
public record ProductListResponse(Guid Id, string Sku, string Name, string? Brand, decimal Price, decimal? PromotionalPrice, int Stock, bool IsActive, string? CategoryName, string? ImageUrl);
public record ProductSearchRequest(string? Q, Guid? CategoryId, decimal? MinPrice, decimal? MaxPrice, bool? InStock, Guid? StoreId, int Page = 1, int PageSize = 20);
public record ProductBatchUpdateRequest(IReadOnlyCollection<Guid> ProductIds, bool? IsActive, decimal? PriceAdjustmentPercent, int? StockDelta);
