using System.Security.Claims;

namespace Equinor.ProCoSys.Common.Misc
{
    public interface IClaimsPrincipalProvider
    {
        ClaimsPrincipal GetCurrentClaimsPrincipal();
    }
}
