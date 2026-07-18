using System.Text.Json;
using n8neiritech.Application.Interfaces;
using StackExchange.Redis;

namespace n8neiritech.Infrastructure.Services;

public class CacheService : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public CacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await _connectionMultiplexer.GetDatabase().StringGetAsync(key);
        if (!value.HasValue)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(value.ToString(), JsonOptions);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
        => _connectionMultiplexer.GetDatabase().StringSetAsync(key, JsonSerializer.Serialize(value, JsonOptions), expiry);

    public Task RemoveAsync(string key, CancellationToken ct = default)
        => _connectionMultiplexer.GetDatabase().KeyDeleteAsync(key);

    public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
        => _connectionMultiplexer.GetDatabase().KeyExistsAsync(key);

    public Task<bool> SetIfNotExistsAsync(string key, string value, TimeSpan expiry, CancellationToken ct = default)
        => _connectionMultiplexer.GetDatabase().StringSetAsync(key, value, expiry, When.NotExists);
}
