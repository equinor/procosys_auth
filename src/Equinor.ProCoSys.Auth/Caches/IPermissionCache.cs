using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Permission;

namespace Equinor.ProCoSys.Auth.Caches
{
    public interface IPermissionCache
    {
        Task<IList<string>> GetPlantIdsWithAccessForUserAsync(Guid userOid);
        Task<bool> HasUserAccessToPlantAsync(string plantId, Guid userOid);
        Task<bool> HasCurrentUserAccessToPlantAsync(string plantId);
        Task<bool> IsAValidPlantForCurrentUserAsync(string plantId);
        Task<string> GetPlantTitleForCurrentUserAsync(string plantId);

        Task<IList<string>> GetPermissionsForUserAsync(string plantId, Guid userOid);

        Task<IList<string>> GetProjectNamesForUserAsync(string plantId, Guid userOid);
        Task<IList<AccessableProject>> GetProjectsForUserAsync(string plantId, Guid userOid);
        Task<bool> IsAValidProjectForUserAsync(string plantId, Guid userOid, string projectName);
        Task<bool> IsAValidProjectForUserAsync(string plantId, Guid userOid, Guid projectGuid);

        Task<IList<string>> GetRestrictionRolesForUserAsync(string plantId, Guid userOid);

        void ClearAll(string plantId, Guid userOid);
    }
}
