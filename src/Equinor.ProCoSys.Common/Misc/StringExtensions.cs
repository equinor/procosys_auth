namespace Equinor.ProCoSys.Common.Misc
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string value) => string.IsNullOrWhiteSpace(value);

        public static string ToPascalCase(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                if (str.Length == 1)
                {
                    return str.ToUpperInvariant();
                }

                var firstLetter = str[0].ToString().ToUpperInvariant();
                return firstLetter + str[1..];
            }
            return str;
        }
    }
}
