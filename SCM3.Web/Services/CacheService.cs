using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace SCM3.Web.Services;

// Redis wrapper for caching rarely-changing lookups — RequestType, RequestStatus,
// Customer, Product, dashboard stats (root CLAUDE.md §9). Concrete class, no interface,
// matching the doc's listing — there's only ever one (Redis-backed) implementation.
public class CacheService(IDistributedCache cache)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await cache.GetStringAsync(key);
        return json is null ? default : JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }

        await cache.SetStringAsync(key, JsonSerializer.Serialize(value, JsonOptions), options);
    }

    public Task RemoveAsync(string key) => cache.RemoveAsync(key);

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var cached = await GetAsync<T>(key);
        if (cached is not null)
        {
            return cached;
        }

        var value = await factory();
        await SetAsync(key, value, expiration);
        return value;
    }
}
