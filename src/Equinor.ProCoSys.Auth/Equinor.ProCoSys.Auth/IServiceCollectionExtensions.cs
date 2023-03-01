using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.Auth
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddPcsAuthIntegration(this IServiceCollection services)
        {

            return services;
        }
    }
}
