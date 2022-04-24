namespace Craftsman.Helpers;

using System;

public static class Extensions
{
    public static string UppercaseFirstLetter(this string value)
    {
        // Check for empty string.
        if (string.IsNullOrEmpty(value))
            return value;
        // Return char and concat substring.
        return char.ToUpper(value[0]) + value.Substring(1);
    }

    public static string LowercaseFirstLetter(this string value)
    {
        // Check for empty string.
        if (string.IsNullOrEmpty(value))
            return value;

        // Return char and concat substring.
        return char.ToLower(value[0]) + value.Substring(1);
    }

    public static string EscapeCurlyBraces(this string value)
    {
        // Check for empty string.
        if (string.IsNullOrEmpty(value))
            return value;

        return value
            .Replace("{", "{{")
            .Replace("}", "}}");
    }

    public static string EscapeSpaces(this string value, string escapeWith = "")
    {
        // Check for empty string.
        if (string.IsNullOrEmpty(value))
            return value;

        return value
            .Replace(" ", escapeWith);
    }

    public static string RemoveLastNewLine(this string input)
    {
        int index = input.LastIndexOf(Environment.NewLine);
        if (index >= 0)
            input = input.Substring(0, index) + input.Substring(index + Environment.NewLine.Length);

        return input;
    }

    public static bool IsGuidPropertyType(this string input)
    {
        return input.Equals("guid", StringComparison.InvariantCultureIgnoreCase);
    }
}
