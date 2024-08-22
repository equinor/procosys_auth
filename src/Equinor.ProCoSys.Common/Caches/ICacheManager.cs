using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Common.Caches
{
    public interface ICacheManager
    {
        Task<T> GetAsync<T>(string key, CancellationToken cancellationToken);
        Task RemoveAsync(string key, CancellationToken cancellationToken);
        Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> fetch, CacheDuration duration, long expiration, CancellationToken cancellationToken);
        Task<List<T>> GetManyAsync<T>(List<string> keys, CancellationToken cancellationToken);
        Task CreateAsync<T>(string key, T item, CacheDuration duration, long expiration, CancellationToken cancellationToken);
    }
}
