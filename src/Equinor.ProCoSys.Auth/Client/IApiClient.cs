using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Auth.Client
{
    public interface IApiClient
    {
        Task<T> TryQueryAndDeserializeAsync<T>(string url, List<KeyValuePair<string, string>> extraHeaders = null, CancellationToken cancellationToken = default);
        Task<T> TryQueryAndDeserializeAsApplicationAsync<T>(string url, List<KeyValuePair<string, string>> extraHeaders = null, CancellationToken cancellationToken = default);
        Task<T> QueryAndDeserializeAsync<T>(string url, List<KeyValuePair<string, string>> extraHeaders = null, CancellationToken cancellationToken = default);
        Task<T> QueryAndDeserializeAsApplicationAsync<T>(string url, List<KeyValuePair<string, string>> extraHeaders = null, CancellationToken cancellationToken = default);
        Task PutAsync(string url, HttpContent content, CancellationToken cancellationToken = default);
        Task PostAsync(string url, HttpContent content, CancellationToken cancellationToken = default);
    }
}
