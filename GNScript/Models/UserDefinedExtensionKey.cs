namespace GNScript.Models;
public class UserDefinedExtensionKey
{
    public string ExtensionName { get; }
    public int NumberOfParameters { get; }

    public UserDefinedExtensionKey(string extensionName, int numberOfParameters)
    {
        ExtensionName = extensionName;
        NumberOfParameters = numberOfParameters;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj is UserDefinedExtensionKey other == false)
            return false;

        return other.ExtensionName == ExtensionName && other.NumberOfParameters == NumberOfParameters;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ExtensionName, NumberOfParameters);
    }
}
