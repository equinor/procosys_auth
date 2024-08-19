using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Auth.Client
{
    /// <summary>
    /// Abstract class to create an authenticated HttpClient to access a foreign API
    /// </summary>
    public abstract class BearerTokenApiClient(
        string clientName,
        IHttpClientFactory httpClientFactory,
        ILogger<BearerTokenApiClient> logger)
    {
        public async Task<T> TryQueryAndDeserializeAsync<T>(string url, CancellationToken cancellationToken, List<KeyValuePair<string, string>> extraHeaders = null)
            => await QueryAndDeserializeAsync<T>(url, true, extraHeaders, cancellationToken);

        public async Task<T> QueryAndDeserializeAsync<T>(string url, CancellationToken cancellationToken, List<KeyValuePair<string, string>> extraHeaders = null)
            => await QueryAndDeserializeAsync<T>(url, false, extraHeaders, cancellationToken);

        public async Task PutAsync(string url, HttpContent content, CancellationToken cancellationToken)
        {
            var httpClient = CreateHttpClient();
            var stopWatch = Stopwatch.StartNew();
            var response = await httpClient.PutAsync(url, content, cancellationToken);
            stopWatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError($"Putting to '{url}' was unsuccessful and took {stopWatch.Elapsed.TotalMilliseconds}ms. Status: {response.StatusCode}");
                throw new Exception();
            }
        }

        public async Task PostAsync(string url, HttpContent content, CancellationToken cancellationToken)
        {
            var httpClient = CreateHttpClient();
            var stopWatch = Stopwatch.StartNew();
            var response = await httpClient.PostAsync(url, content, cancellationToken);
            stopWatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Posting to '{url}' was unsuccessful and took {stopWatch.Elapsed.TotalMilliseconds}ms. Status: {response.StatusCode}");
            }
        }

        private async Task<T> QueryAndDeserializeAsync<T>(
            string url,
            bool tryGet,
            List<KeyValuePair<string, string>> extraHeaders,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (url.Length > 2000)
            {
                throw new ArgumentException("url exceed max 2000 characters", nameof(url));
            }

            var httpClient = CreateHttpClient(extraHeaders);

            var stopWatch = Stopwatch.StartNew();
            var response = await httpClient.GetAsync(url, cancellationToken);
            stopWatch.Stop();

            var msg = $"{stopWatch.Elapsed.TotalSeconds}s elapsed when requesting '{url}'. Status: {response.StatusCode}";
            if (!response.IsSuccessStatusCode)
            {
                if (tryGet && response.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.LogWarning(msg);
                    return default;
                }
                logger.LogError(msg);
                throw new Exception($"Requesting '{url}' was unsuccessful. Status={response.StatusCode}");
            }

            logger.LogInformation(msg);

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var result = await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);
            return result;
        }

        public HttpClient CreateHttpClient(List<KeyValuePair<string, string>> extraHeaders = null)
        {
            var httpClient = httpClientFactory.CreateClient(clientName);
            httpClient.Timeout = TimeSpan.FromSeconds(200); //TODO: Technical debth, add this value to config

            extraHeaders?.ForEach(h => httpClient.DefaultRequestHeaders.Add(h.Key, h.Value));

            return httpClient;
        }
    }
}
