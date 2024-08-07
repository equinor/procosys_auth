using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Equinor.ProCoSys.Common.Caches;

public sealed class DistributedCacheManager(IDistributedCache distributedCache) : ICacheManager
{
    public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken) where T : class
        => await GetObjectFromCacheAsync<T>(key, cancellationToken);

    public async Task RemoveAsync(string key, CancellationToken cancellationToken)
        => await distributedCache.RemoveAsync(key, cancellationToken);

    public async Task<T> GetOrCreateAsync<T>(
        string key, 
        Func<Task<T>> fetch, 
        CacheDuration duration, 
        long expiration,
        CancellationToken cancellationToken) where T : class
    {
        var item = await GetObjectFromCacheAsync<T>(key, cancellationToken);
        if (item is not null)
        {
            return item;
        }

        item = await fetch();
        var serialized = JsonSerializer.Serialize(item);
        await distributedCache.SetStringAsync(key, serialized,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = GetEntryCacheDuration(duration, expiration)
            }, cancellationToken);

        return item;
    }

    private async Task<T> GetObjectFromCacheAsync<T>(string key, CancellationToken cancellationToken) where T : class
    {
        var entry = await distributedCache.GetStringAsync(key, cancellationToken);
        if (entry is null)
        {
            return null;
        }
        var deserialized = JsonSerializer.Deserialize<T>(entry);

        return deserialized;
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
