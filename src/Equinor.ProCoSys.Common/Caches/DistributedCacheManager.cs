using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Common.Caches;

public sealed class DistributedCacheManager(
    IDistributedCache distributedCache,
    ILogger<DistributedCacheManager> logger) : ICacheManager
{
    public Task<T> GetAsync<T>(string key, CancellationToken cancellationToken) where T : class
        => GetObjectFromCacheAsync<T>(key, cancellationToken);

    public Task RemoveAsync(string key, CancellationToken cancellationToken)
        => distributedCache.RemoveAsync(key, cancellationToken);

    public async Task<T> GetOrCreateAsync<T>(
        string key, 
        Func<CancellationToken, Task<T>> fetch, 
        CacheDuration duration, 
        long expiration,
        CancellationToken cancellationToken) where T : class
    {
        var item = await GetObjectFromCacheAsync<T>(key, cancellationToken);
        if (item is not null)
        {
            return item;
        }

        item = await fetch(cancellationToken);
        var serialized = JsonSerializer.Serialize(item);
        await distributedCache.SetStringAsync(key, serialized,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = GetEntryCacheDuration(duration, expiration)
            }, cancellationToken);

        logger.LogInformation("Added {Key} to cache", key);
        return item;
    }

    private async Task<T> GetObjectFromCacheAsync<T>(string key, CancellationToken cancellationToken) where T : class
    {
        var entry = await distributedCache.GetStringAsync(key, cancellationToken);
        if (entry is null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(entry);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to deserialize cached value for key {key} as {typeof(T)}", ex);
        }
    }

    private static TimeSpan GetEntryCacheDuration(CacheDuration duration, long expiration) =>
        duration switch
        {
            CacheDuration.Seconds => TimeSpan.FromSeconds(expiration),
            CacheDuration.Minutes => TimeSpan.FromMinutes(expiration),
            CacheDuration.Hours => TimeSpan.FromHours(expiration),
            _ => throw new ArgumentOutOfRangeException(nameof(duration), duration, null)
        };
}
