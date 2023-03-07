using System;

namespace Equinor.ProCoSys.Common.Misc
{
    public interface ICurrentUserSetter
    {
        void SetCurrentUserOid(Guid oid);
    }
}
