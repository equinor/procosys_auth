using System;

namespace Equinor.ProCoSys.Common.Misc
{
    public interface ICurrentUserProvider
    {
        Guid GetCurrentUserOid();
        bool HasCurrentUser { get; }
    }
}
