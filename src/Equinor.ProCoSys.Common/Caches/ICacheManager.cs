using System;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Common.Caches
{
    public interface ICacheManager
    {
        Task<T> GetAsync<T>(string key, CancellationToken cancellationToken) where T : class;
        Task RemoveAsync(string key, CancellationToken cancellationToken);
        Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> fetch, CacheDuration duration, long expiration, CancellationToken cancellationToken) where T : class;
    }
}
