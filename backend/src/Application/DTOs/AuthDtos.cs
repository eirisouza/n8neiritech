using n8neiritech.Domain.Enums;

namespace n8neiritech.Application.DTOs;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string AccessToken, string RefreshToken, int ExpiresIn, UserDto User);
public record RefreshTokenRequest(string RefreshToken);
public record UserDto(Guid Id, string Name, string Email, UserRole Role, Guid TenantId, Guid? StoreId);
