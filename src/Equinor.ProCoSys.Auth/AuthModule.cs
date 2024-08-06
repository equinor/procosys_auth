using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.Auth.Misc;
using Equinor.ProCoSys.Auth.Permission;
using Equinor.ProCoSys.Auth.Person;
using Equinor.ProCoSys.Common.Caches;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Time;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.Auth
{
    public static class AuthModule
    {
        public static IServiceCollection AddPcsAuthIntegration(this IServiceCollection services)
        {
            TimeService.SetProvider(new SystemTimeProvider());

            services.AddScoped<IClaimsTransformation, ClaimsTransformation>();

            services.AddScoped<PlantProvider>();
            services.AddScoped<IPlantProvider>(x => x.GetRequiredService<PlantProvider>());
            services.AddScoped<IPlantSetter>(x => x.GetRequiredService<PlantProvider>());
            services.AddScoped<IMainApiClientForUser, MainApiClientForUser>();
            services.AddScoped<IMainApiClientForApplication, MainApiClientForApplication>();
            services.AddScoped<IPersonApiService, MainApiPersonService>();
            services.AddScoped<IPermissionApiService, MainApiPermissionService>();
            services.AddScoped<IPersonCache, PersonCache>();
            services.AddScoped<IPermissionCache, PermissionCache>();
            services.AddScoped<IClaimsPrincipalProvider, ClaimsPrincipalProvider>();
            services.AddScoped<CurrentUserProvider>();
            services.AddScoped<ICurrentUserProvider>(x => x.GetRequiredService<CurrentUserProvider>());
            services.AddScoped<ICurrentUserSetter>(x => x.GetRequiredService<CurrentUserProvider>());
            services.AddScoped<IRestrictionRolesChecker, RestrictionRolesChecker>();

            // Singleton - Created the first time they are requested
            services.AddSingleton<ICacheManager, CacheManager>();

            AddMainApiHttpClients(services);

            return services;
        }

        private static void AddMainApiHttpClients(IServiceCollection services)
        {
            services.AddTransient<MainApiForUserBearerTokenHandler>();
            services.AddTransient<MainApiForApplicationBearerTokenHandler>();

            services.AddHttpClient(MainApiClientForUser.ClientName)
                .AddHttpMessageHandler<MainApiForUserBearerTokenHandler>();

            services.AddHttpClient(MainApiClientForApplication.ClientName)
                .AddHttpMessageHandler<MainApiForApplicationBearerTokenHandler>();
        }
    }
}
