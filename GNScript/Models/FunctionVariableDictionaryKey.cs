namespace GNScript.Models;
public class FunctionVariableDictionaryKey
{
    public string VariableName { get; } = string.Empty;
    public FunctionDictionaryKey FunctionKey { get; } = null;

    public FunctionVariableDictionaryKey(string variableName)
    {
        VariableName = variableName;
    }

    public FunctionVariableDictionaryKey(FunctionNode functionNode)
    {
        FunctionKey = new(functionNode);
    }

    public FunctionVariableDictionaryKey(FunctionCallNode functionCallNode)
    {
        FunctionKey = new(functionCallNode);
    }

    public override bool Equals(object? obj)
    {
        if (obj is FunctionVariableDictionaryKey functionVariableDictionaryKey)
        {
            return functionVariableDictionaryKey.VariableName == VariableName &&
                functionVariableDictionaryKey.FunctionKey == FunctionKey;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(VariableName, FunctionKey?.GetHashCode());
    }

    public static bool operator ==(FunctionVariableDictionaryKey left, FunctionVariableDictionaryKey right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }
        if (left is null || right is null)
        {
            return false;
        }
        return left.Equals(right);
    }

    public static bool operator !=(FunctionVariableDictionaryKey left, FunctionVariableDictionaryKey right)
    {
        return !(left == right);
    }
}
