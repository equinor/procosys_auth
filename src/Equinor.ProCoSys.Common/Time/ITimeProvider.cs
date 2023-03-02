using System;

namespace Equinor.ProCoSys.Common.Time
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }
}
