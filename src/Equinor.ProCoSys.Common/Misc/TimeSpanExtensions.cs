using System;

namespace Equinor.ProCoSys.Common.Misc
{
    public static class TimeSpanExtensions
    {
        public static int Weeks(this TimeSpan span) => span.Days / 7;
    }
}
