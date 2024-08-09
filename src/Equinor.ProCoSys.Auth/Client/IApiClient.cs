using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Auth.Client
{
    public interface IApiClient
    {
        Task<T> TryQueryAndDeserializeAsync<T>(string url, CancellationToken cancellationToken, List<KeyValuePair<string, string>> extraHeaders = null);
        Task<T> QueryAndDeserializeAsync<T>(string url, CancellationToken cancellationToken, List<KeyValuePair<string, string>> extraHeaders = null);
        Task PutAsync(string url, HttpContent content, CancellationToken cancellationToken);
        Task PostAsync(string url, HttpContent content, CancellationToken cancellationToken);
    }
}
