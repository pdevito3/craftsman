namespace Craftsman
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class Extensions
    {
        public static string UppercaseFirstLetter(this string value)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(value[0]) + value.Substring(1);
        }
    }
}
