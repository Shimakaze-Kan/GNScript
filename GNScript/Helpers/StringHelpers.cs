namespace GNScript.Helpers;
public static class StringHelpers
{
    public static string TrimStart(this string source, string trimString)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(trimString))
        {
            return source;
        }

        while (source.StartsWith(trimString))
        {
            source = source[trimString.Length..];
        }

        return source;
    }

    public static string TrimEnd(this string source, string trimString)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(trimString))
        {
            return source;
        }

        while (source.EndsWith(trimString))
        {
            source = source[..^trimString.Length];
        }

        return source;
    }

    public static int CountOccurrences(this string source, string subString)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(subString))
        {
            return 0;
        }

        int count = 0;
        int index = 0;

        while ((index = source.IndexOf(subString, index)) != -1)
        {
            count++;
            index += subString.Length;
        }

        return count;
    }
}
