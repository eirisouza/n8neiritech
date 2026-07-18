namespace n8neiritech.Application.DTOs;

public record CreateTenantRequest(string Name, string Slug, string? LogoUrl, string? PrimaryColor, string? Plan, DateTime? PlanExpiresAt);
public record UpdateTenantRequest(string Name, string Slug, string? LogoUrl, string? PrimaryColor, bool IsActive, string? Plan, DateTime? PlanExpiresAt);
public record TenantResponse(Guid Id, string Name, string Slug, string? LogoUrl, string? PrimaryColor, bool IsActive, string? Plan, DateTime? PlanExpiresAt);
