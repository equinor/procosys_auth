using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Auth.Permission
{
    public interface IPermissionApiService
    {
        Task<List<AccessablePlant>> GetAllPlantsForUserAsync(Guid azureOid, CancellationToken cancellationToken);
        Task<List<string>> GetPermissionsForCurrentUserAsync(string plantId, CancellationToken cancellationToken);
        Task<List<AccessableProject>> GetAllOpenProjectsForCurrentUserAsync(string plantId, CancellationToken cancellationToken);
        Task<List<string>> GetRestrictionRolesForCurrentUserAsync(string plantId, CancellationToken cancellationToken);
        Task<UserPlantPermissionData> GetUserPlantPermissionDataAsync(Guid userOid, string plantId, CancellationToken cancellationToken);
    }
}
