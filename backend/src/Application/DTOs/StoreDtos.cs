namespace n8neiritech.Application.DTOs;

public record CreateStoreRequest(Guid TenantId, string Name, string? Description, string? LogoUrl, string? Phone, string? Email, string? Website, string? AddressLine, string? City, string? State, string? PostalCode, string? Country, string? BusinessType);
public record UpdateStoreRequest(string Name, string? Description, string? LogoUrl, string? Phone, string? Email, string? Website, string? AddressLine, string? City, string? State, string? PostalCode, string? Country, bool IsActive, string? BusinessType);
public record StoreResponse(Guid Id, Guid TenantId, string Name, string? Description, string? LogoUrl, string? Phone, string? Email, string? Website, string? AddressLine, string? City, string? State, string? PostalCode, string? Country, bool IsActive, string? BusinessType);
public record BusinessHourDto(Guid Id, Guid StoreId, DayOfWeek DayOfWeek, bool IsOpen, TimeOnly OpenTime, TimeOnly CloseTime);
