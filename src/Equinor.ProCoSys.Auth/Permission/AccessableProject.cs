using System;
using System.Diagnostics;

namespace Equinor.ProCoSys.Auth.Permission
{
    [DebuggerDisplay("{Name} {ProCoSysGuid} {HasAccess}")]
    public class AccessableProject
    {
        public Guid ProCoSysGuid { get; set; }
        public string Name { get; set; }
        public bool HasAccess { get; set; }
    }
}
