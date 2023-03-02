using Equinor.ProCoSys.Common.Time;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.Auth
{
    public static class AuthModule
    {
        public static IServiceCollection AddPcsAuthIntegration(this IServiceCollection services)
        {
            TimeService.SetProvider(new SystemTimeProvider());

            return services;
        }
    }
}
