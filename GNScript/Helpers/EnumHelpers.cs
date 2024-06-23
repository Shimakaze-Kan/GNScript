namespace GNScript.Helpers;
public static class EnumHelpers
{
    public static string[] GetEnumNamesLowercase<T>() where T : Enum
    {
        var names = Enum.GetNames(typeof(T));
        for (int i = 0; i < names.Length; i++)
        {
            names[i] = names[i].ToLowerInvariant();
        }
        return names;
    }

    public static bool EqualsIgnoreCase<T>(string input, T enumValue) where T : Enum
    {
        return string.Equals(input, Enum.GetName(typeof(T), enumValue), StringComparison.OrdinalIgnoreCase);
    }
}
