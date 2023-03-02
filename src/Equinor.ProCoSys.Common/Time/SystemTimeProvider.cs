using System;

namespace Equinor.ProCoSys.Common.Time
{
    public class SystemTimeProvider : ITimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
