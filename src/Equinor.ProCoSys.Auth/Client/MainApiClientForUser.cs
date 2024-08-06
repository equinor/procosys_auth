using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Auth.Client
{
    /// <summary>
    /// Implementation of the abstract BearerTokenApiClient to access Main Api.
    /// The implementation of IMainApiAuthenticator refer to the correct scope for Main Api
    /// </summary>
    public class MainApiClientForUser : BearerTokenApiClient, IMainApiClientForUser
    {
        public static string ClientName = "MainApiClientForUser";

        public MainApiClientForUser(IHttpClientFactory httpClientFactory,
            ILogger<MainApiClientForUser> logger) : base(ClientName, httpClientFactory, logger)
        {
        }
    }
}
