using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Person;

namespace Equinor.ProCoSys.Auth.Authorization
{
    // Must be implemented and added to DI Container in each
    // solution using Equinor.ProCoSys.Auth.AuthorizationClaimsTransformation
    // For solutions without local Person repo, the implemented method should just return false
    public interface ILocalPersonRepository
    {
        Task<bool> ExistsAsync(Guid userOid);
        Task<ProCoSysPerson> GetAsync(Guid userOid);
    }
}