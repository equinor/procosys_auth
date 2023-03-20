using System;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Auth.Authorization
{
    public interface ILocalPersonRepository
    {
        Task<bool> ExistsAsync(Guid userOid);
    }
}