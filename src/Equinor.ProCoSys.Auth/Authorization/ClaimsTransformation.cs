using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Person;
using Equinor.ProCoSys.Common.Misc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Auth.Authorization
{
    /// <summary>
    /// Implement IClaimsTransformation to extend the ClaimsPrincipal with claims to be used during authorization.
    /// Claims added only for authenticated users. User must exist in ProCoSys
    ///  * If ProCoSys user is a superuser, a claim of type ClaimTypes.Role with value SUPERUSER is added.
    ///    The SUPERUSER claim is added regardless if request is a plant request or not
    /// For requests handling a valid plant for user, these types of claims are added:
    ///  * ClaimTypes.Role claim for each user permission (such as TAG/READ)
    ///  * ClaimTypes.UserData claim for each project user has access to. These claim name start with ProjectPrefix
    ///  * ClaimTypes.UserData claim for each restriction role for user. These claim name start with RestrictionRolePrefix
    ///    (Restriction role = "%" means "User has no restriction roles")
    /// </summary>
    public class ClaimsTransformation : IClaimsTransformation
    {
        public const string Superuser = "SUPERUSER";
        public const string PersonExist = "Person-Exists-";
        public const string ClaimsIssuer = "ProCoSys";
        public const string ProjectPrefix = "PCS_Project##";
        public const string RestrictionRolePrefix = "PCS_RestrictionRole##";
        public const string NoRestrictions = "%";

        private readonly ILocalPersonRepository _localPersonRepository;
        private readonly IPersonCache _personCache;
        private readonly IPlantProvider _plantProvider;
        private readonly IPermissionCache _permissionCache;
        private readonly ILogger<ClaimsTransformation> _logger;
        private readonly IOptionsMonitor<MainApiAuthenticatorOptions> _authenticatorOptions;

        public ClaimsTransformation(
            ILocalPersonRepository localPersonRepository,
            IPersonCache personCache,
            IPlantProvider plantProvider,
            IPermissionCache permissionCache,
            ILogger<ClaimsTransformation> logger,
            IOptionsMonitor<MainApiAuthenticatorOptions> authenticatorOptions)
        {
            _localPersonRepository = localPersonRepository;
            _personCache = personCache;
            _plantProvider = plantProvider;
            _permissionCache = permissionCache;
            _logger = logger;
            _authenticatorOptions = authenticatorOptions;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            _logger.LogInformation("----- {Name} start", GetType().Name);

            // Can't use CurrentUserProvider here. Middleware setting current user not called yet. 
            var userOid = principal.Claims.TryGetOid();
            if (!userOid.HasValue)
            {
                _logger.LogInformation("----- {Name} early exit, not authenticated yet", GetType().Name);
                return principal;
            }

            var proCoSysPerson = await GetProCoSysPersonAsync(userOid.Value);
            if (proCoSysPerson is null)
            {
                _logger.LogInformation("----- {Name} early exit, {UserOid} don\'t exists in ProCoSys", GetType().Name, userOid);
                return principal;
            }
            var claimsIdentity = GetOrCreateClaimsIdentityForThisIssuer(principal);
            
            AddPersonExistsClaim(claimsIdentity, proCoSysPerson.AzureOid);
            
            if (proCoSysPerson.Super)
            {
                AddSuperRoleToIdentity(claimsIdentity);
                _logger.LogInformation("----- {Name}: {UserOid} logged in as a ProCoSys superuser", GetType().Name, userOid);
            }

            var plantId = _plantProvider.Plant;
            if (string.IsNullOrEmpty(plantId))
            {
                _logger.LogInformation("----- {Name} early exit, not a plant request", GetType().Name);
                return principal;
            }

            if (!await _permissionCache.HasUserAccessToPlantAsync(plantId, userOid.Value))
            {
                _logger.LogInformation("----- {Name} early exit, not a valid plant for user", GetType().Name);
                return principal;
            }
            
            await AddRoleForAllPermissionsToIdentityAsync(claimsIdentity, plantId, userOid.Value);
            if (!_authenticatorOptions.CurrentValue.DisableProjectUserDataClaims)
            {
                await AddUserDataClaimForAllOpenProjectsToIdentityAsync(claimsIdentity, plantId, userOid.Value);
            }
            if (!_authenticatorOptions.CurrentValue.DisableRestrictionRoleUserDataClaims)
            {
                await AddUserDataClaimForAllRestrictionRolesToIdentityAsync(claimsIdentity, plantId, userOid.Value);
            }

            _logger.LogInformation("----- {Name} completed", GetType().Name);
            return principal;
        }

        private static void AddPersonExistsClaim(ClaimsIdentity claimsIdentity, string azureOid)
        {
            claimsIdentity.AddClaim(new Claim(ClaimTypes.UserData, $"{PersonExist}{azureOid}"));
        }

        public static string GetProjectClaimValue(string projectName) => $"{ProjectPrefix}{projectName}";
        public static string GetProjectClaimValue(Guid projectGuid) => $"{ProjectPrefix}{projectGuid}";

        public static string GetRestrictionRoleClaimValue(string restrictionRole) => $"{RestrictionRolePrefix}{restrictionRole}";

        private async Task<ProCoSysPerson> GetProCoSysPersonAsync(Guid userOid)
        {
            // check if user exists in local repository before checking
            // cache which get user from ProCoSys
            var proCoSysPerson = await _localPersonRepository.GetAsync(userOid);
            if (proCoSysPerson is not null)
            {
                return proCoSysPerson;
            }

            return await _personCache.GetAsync(userOid);
        }

        private ClaimsIdentity GetOrCreateClaimsIdentityForThisIssuer(ClaimsPrincipal principal)
        {
            var identity = principal.Identities.SingleOrDefault(i => i.Label == ClaimsIssuer);
            if (identity == null)
            {
                identity = new ClaimsIdentity { Label = ClaimsIssuer };
                principal.AddIdentity(identity);
            }
            else
            {
                ClearOldClaims(identity);
            }

            return identity;
        }

        private void ClearOldClaims(ClaimsIdentity identity)
        {
            var oldClaims = identity.Claims.Where(c => c.Issuer == ClaimsIssuer).ToList();
            oldClaims.ForEach(identity.RemoveClaim);
        }

        private void AddSuperRoleToIdentity(ClaimsIdentity claimsIdentity)
        {
            claimsIdentity.AddClaim(CreateClaim(ClaimTypes.Role, Superuser));
        }

        private async Task AddRoleForAllPermissionsToIdentityAsync(ClaimsIdentity claimsIdentity, string plantId, Guid userOid)
        {
            var permissions = await _permissionCache.GetPermissionsForUserAsync(plantId, userOid);
            permissions?.ToList().ForEach(
                permission => claimsIdentity.AddClaim(CreateClaim(ClaimTypes.Role, permission)));
        }

        private async Task AddUserDataClaimForAllOpenProjectsToIdentityAsync(ClaimsIdentity claimsIdentity, string plantId, Guid userOid)
        {
            var projects = await _permissionCache.GetProjectsForUserAsync(plantId, userOid);
            projects?.ToList().ForEach(project =>
            {
                claimsIdentity.AddClaim(CreateClaim(ClaimTypes.UserData, GetProjectClaimValue(project.Name)));
                claimsIdentity.AddClaim(CreateClaim(ClaimTypes.UserData, GetProjectClaimValue(project.ProCoSysGuid)));
            });
        }

        private async Task AddUserDataClaimForAllRestrictionRolesToIdentityAsync(ClaimsIdentity claimsIdentity, string plantId, Guid userOid)
        {
            var restrictions = await _permissionCache.GetRestrictionRolesForUserAsync(plantId, userOid);
            restrictions?.ToList().ForEach(
                r => claimsIdentity.AddClaim(CreateClaim(ClaimTypes.UserData, GetRestrictionRoleClaimValue(r))));
        }

        private static Claim CreateClaim(string claimType, string claimValue)
            => new(claimType, claimValue, null, ClaimsIssuer);
    }
}
