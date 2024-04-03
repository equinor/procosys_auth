using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authentication;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Auth.Client
{
    /// <summary>
    /// Abstract class to create an authenticated HttpClient to access a foreign API
    /// The class is made abstract to force creating an implementation of a IBearerTokenProvider
    /// which is implemented to use a particular configuration for the particular foreign API
    /// </summary>
    public abstract class BearerTokenApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBearerTokenProvider _bearerTokenProvider;

        protected readonly ILogger<BearerTokenApiClient> _logger;

        public BearerTokenApiClient(
            IHttpClientFactory httpClientFactory,
            IBearerTokenProvider bearerTokenProvider,
            ILogger<BearerTokenApiClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _bearerTokenProvider = bearerTokenProvider;
            _logger = logger;
        }

        public async Task<T> TryQueryAndDeserializeAsync<T>(string url, List<KeyValuePair<string, string>> extraHeaders = null, CancellationToken cancellationToken = default)
            => await QueryAndDeserializeAsync<T>(url, true, extraHeaders, cancellationToken);

        public async Task<T> QueryAndDeserializeAsync<T>(string url, List<KeyValuePair<string, string>> extraHeaders = null, CancellationToken cancellationToken = default)
            => await QueryAndDeserializeAsync<T>(url, false, extraHeaders, cancellationToken);

        public async Task<T> TryQueryAndDeserializeAsApplicationAsync<T>(string url, List<KeyValuePair<string, string>> extraHeaders = null, CancellationToken cancellationToken = default)
            => await QueryAndDeserializeAsApplicationAsync<T>(url, true, extraHeaders, cancellationToken);

        public async Task<T> QueryAndDeserializeAsApplicationAsync<T>(string url, List<KeyValuePair<string, string>> extraHeaders = null, CancellationToken cancellationToken = default)
            => await QueryAndDeserializeAsApplicationAsync<T>(url, false, extraHeaders, cancellationToken);

        public async Task PutAsync(string url, HttpContent content, CancellationToken cancellationToken = default)
        {
            var httpClient = await CreateHttpClientAsync(null, cancellationToken);

            var stopWatch = Stopwatch.StartNew();
            var response = await httpClient.PutAsync(url, content, cancellationToken);
            stopWatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Putting to '{url}' was unsuccessful and took {stopWatch.Elapsed.TotalMilliseconds}ms. Status: {response.StatusCode}");
                throw new Exception();
            }
        }

        public async Task PostAsync(string url, HttpContent content, CancellationToken cancellationToken = default)
        {
            var httpClient = await CreateHttpClientAsync(null, cancellationToken);

            var stopWatch = Stopwatch.StartNew();
            var response = await httpClient.PostAsync(url, content, cancellationToken);
            stopWatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Posting to '{url}' was unsuccessful and took {stopWatch.Elapsed.TotalMilliseconds}ms. Status: {response.StatusCode}");
                throw new Exception();
            }
        }

        private async Task<T> QueryAndDeserializeAsApplicationAsync<T>(string url, bool tryGet, List<KeyValuePair<string, string>> extraHeaders, CancellationToken cancellationToken = default)
        {
            var oldAuthType = _bearerTokenProvider.AuthenticationType;
            _bearerTokenProvider.AuthenticationType = AuthenticationType.AsApplication;
            try
            {
                return await QueryAndDeserializeAsync<T>(url, tryGet, extraHeaders, cancellationToken);
            }
            finally
            {
                _bearerTokenProvider.AuthenticationType = oldAuthType;
            }
        }

        private async Task<T> QueryAndDeserializeAsync<T>(
            string url,
            bool tryGet,
            List<KeyValuePair<string, string>> extraHeaders = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (url.Length > 2000)
            {
                throw new ArgumentException("url exceed max 2000 characters", nameof(url));
            }

            var httpClient = await CreateHttpClientAsync(extraHeaders, cancellationToken);

            var stopWatch = Stopwatch.StartNew();
            var response = await httpClient.GetAsync(url, cancellationToken);
            stopWatch.Stop();

            var msg = $"{stopWatch.Elapsed.TotalSeconds}s elapsed when requesting '{url}'. Status: {response.StatusCode}";
            if (!response.IsSuccessStatusCode)
            {
                if (tryGet && response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning(msg);
                    return default;
                }
                _logger.LogError(msg);
                throw new Exception($"Requesting '{url}' was unsuccessful. Status={response.StatusCode}");
            }

            _logger.LogInformation(msg);

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var result = await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);
            return result;
        }

        public async ValueTask<HttpClient> CreateHttpClientAsync(List<KeyValuePair<string, string>> extraHeaders = null, CancellationToken cancellationToken = default)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(200); //TODO: Technical debth, add this value to config

            var bearerToken = await _bearerTokenProvider.GetBearerTokenAsync(cancellationToken);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            if (extraHeaders != null)
            {
                extraHeaders.ForEach(h => httpClient.DefaultRequestHeaders.Add(h.Key, h.Value));
            }

            return httpClient;
        }
    }
}
