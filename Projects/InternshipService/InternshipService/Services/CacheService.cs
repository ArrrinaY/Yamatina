using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace InternshipService.Services;

public class CacheService(IDistributedCache? cache) : ICacheService
{
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        if (cache == null) return null;
        
        var value = await cache.GetStringAsync(key);
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>(value);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        if (cache == null) return;
        
        var json = JsonSerializer.Serialize(value);
        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration;
        }
        
        await cache.SetStringAsync(key, json, options);
    }

    public async Task RemoveAsync(string key)
    {
        if (cache == null) return;
        await cache.RemoveAsync(key);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        await Task.CompletedTask;
    }
}

