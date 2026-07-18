using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using n8neiritech.Application.Common;
using n8neiritech.Application.DTOs;
using n8neiritech.Application.Interfaces;
using n8neiritech.Domain.Entities;

namespace n8neiritech.Application.Services;

public class AuthService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private readonly IApplicationDbContext _context;
    private readonly AuthOptions _options;

    public AuthService(IApplicationDbContext context, AuthOptions options)
    {
        _context = context;
        _options = options;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail && x.IsActive, ct)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        var refreshToken = await RotateRefreshTokenAsync(user, null, ct);
        await _context.SaveChangesAsync(ct);
        return BuildLoginResponse(user, refreshToken.Token);
    }

    public async Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var token = await _context.RefreshTokens.Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (token.IsRevoked || token.ExpiresAt <= DateTime.UtcNow || !token.User.IsActive)
        {
            throw new UnauthorizedAccessException("Refresh token expired or revoked.");
        }

        var replacement = await RotateRefreshTokenAsync(token.User, token, ct);
        await _context.SaveChangesAsync(ct);
        return BuildLoginResponse(token.User, replacement.Token);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var token = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken, ct);
        if (token is null)
        {
            return;
        }

        token.IsRevoked = true;
        await _context.SaveChangesAsync(ct);
    }

    public string CreateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("tenant_id", user.TenantId.ToString())
        };

        if (user.StoreId.HasValue)
        {
            claims.Add(new Claim("store_id", user.StoreId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(string password, string passwordHash)
    {
        var parts = passwordHash.Split('.', 3);
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[1]);
        var expectedHash = Convert.FromBase64String(parts[2]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private LoginResponse BuildLoginResponse(User user, string refreshToken)
    {
        var accessToken = CreateJwtToken(user);
        return new LoginResponse(
            accessToken,
            refreshToken,
            _options.ExpirationMinutes * 60,
            new UserDto(user.Id, user.Name, user.Email, user.Role, user.TenantId, user.StoreId));
    }

    private async Task<RefreshToken> RotateRefreshTokenAsync(User user, RefreshToken? existing, CancellationToken ct)
    {
        if (existing is not null)
        {
            existing.IsRevoked = true;
        }

        var token = new RefreshToken
        {
            UserId = user.Id,
            User = user,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenExpirationDays),
            IsRevoked = false,
            ReplacedByToken = null
        };

        if (existing is not null)
        {
            existing.ReplacedByToken = token.Token;
        }

        await _context.RefreshTokens.AddAsync(token, ct);
        return token;
    }
}
