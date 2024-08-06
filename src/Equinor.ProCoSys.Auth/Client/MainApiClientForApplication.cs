using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Auth.Client
{
    /// <summary>
    /// Implementation of the abstract BearerTokenApiClient to access Main Api.
    /// The implementation of IMainApiAuthenticator refer to the correct scope for Main Api
    /// </summary>
    public class MainApiClientForApplication : BearerTokenApiClient, IMainApiClientForApplication
    {
        public static string ClientName = "MainApiClientForApplication";

        public MainApiClientForApplication(IHttpClientFactory httpClientFactory,
            ILogger<MainApiClientForApplication> logger) : base(ClientName, httpClientFactory, logger)
        {
        }
    }
}
