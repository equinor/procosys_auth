using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.Auth
{
    public static class AuthModule
    {
        public static IServiceCollection AddPcsAuthIntegration(this IServiceCollection services)
        {

            return services;
        }
    }
}
