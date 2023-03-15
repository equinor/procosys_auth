namespace Equinor.ProCoSys.Common.Misc
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string value) => string.IsNullOrWhiteSpace(value);
    }
}
