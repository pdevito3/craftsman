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
        public static string LowercaseFirstLetter(this string value)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToLower(value[0]) + value.Substring(1);
        }

        public static string RemoveLastNewLine(this string input)
        {
            int index = input.LastIndexOf(Environment.NewLine);
            if (index >= 0)
                input = input.Substring(0, index) + input.Substring(index + Environment.NewLine.Length);

            return input;
        }
    }
}
