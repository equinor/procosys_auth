using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Permission;

namespace Equinor.ProCoSys.Auth.Caches
{
    public interface IPermissionCache
    {
        Task<IList<string>> GetPlantIdsWithAccessForUserAsync(Guid userOid, CancellationToken cancellationToken);
        Task<bool> HasUserAccessToPlantAsync(string plantId, Guid userOid, CancellationToken cancellationToken);
        Task<bool> HasCurrentUserAccessToPlantAsync(string plantId, CancellationToken cancellationToken);
        Task<bool> IsAValidPlantForCurrentUserAsync(string plantId, CancellationToken cancellationToken);
        Task<string> GetPlantTitleForCurrentUserAsync(string plantId, CancellationToken cancellationToken);

        Task<IList<string>> GetPermissionsForUserAsync(string plantId, Guid userOid, CancellationToken cancellationToken);

        Task<IList<string>> GetProjectNamesForUserAsync(string plantId, Guid userOid, CancellationToken cancellationToken);
        Task<IList<AccessableProject>> GetProjectsForUserAsync(string plantId, Guid userOid, CancellationToken cancellationToken);
        Task<bool> IsAValidProjectForUserAsync(string plantId, Guid userOid, string projectName, CancellationToken cancellationToken);
        Task<bool> IsAValidProjectForUserAsync(string plantId, Guid userOid, Guid projectGuid, CancellationToken cancellationToken);

        Task<IList<string>> GetRestrictionRolesForUserAsync(string plantId, Guid userOid, CancellationToken cancellationToken);

        Task ClearAllAsync(string plantId, Guid userOid, CancellationToken cancellationToken);
    }
}
