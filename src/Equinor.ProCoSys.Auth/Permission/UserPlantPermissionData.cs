using System;
using System.Linq;

namespace Equinor.ProCoSys.Auth.Permission;

public readonly record struct UserPlantPermissionData(
    Guid Oid,
    string Plant,
    AccessablePlant[] AllPlantsForUser,
    string[] Permissions,
    AccessableProject[] Projects,
    string[] RestrictionRoles
)
{
    public bool HasAccessToPlant(string plantId) => AllPlantsForUser.Any(p => p.Id == plantId && p.HasAccess);
}