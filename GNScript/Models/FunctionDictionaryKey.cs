using GNScript.Helpers;

namespace GNScript.Models;
public class FunctionDictionaryKey
{
    public string FunctionName { get; }
    public int FunctionParameterCount { get; }

    public FunctionDictionaryKey(FunctionNode functionNode)
    {
        ExceptionsHelper.FailIfTrue(functionNode == null, nameof(functionNode));
        ExceptionsHelper.FailIfTrue(string.IsNullOrEmpty(functionNode.Name), nameof(functionNode.Name));

        FunctionName = functionNode.Name;
        FunctionParameterCount = functionNode.Parameters.Count;
    }
    
    public FunctionDictionaryKey(FunctionCallNode functionCallNode)
    {
        ExceptionsHelper.FailIfTrue(functionCallNode == null, nameof(functionCallNode));
        ExceptionsHelper.FailIfTrue(string.IsNullOrEmpty(functionCallNode.Name), nameof(functionCallNode.Name));

        FunctionName = functionCallNode.Name;
        FunctionParameterCount = functionCallNode.Arguments.Count;
    }

    public override bool Equals(object? obj)
    {
        if (obj is FunctionDictionaryKey functionDictionaryKey)
        {
            return functionDictionaryKey.FunctionName == FunctionName && functionDictionaryKey.FunctionParameterCount == FunctionParameterCount;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FunctionName, FunctionParameterCount);
    }

    public static bool operator ==(FunctionDictionaryKey left, FunctionDictionaryKey right)
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

    public static bool operator !=(FunctionDictionaryKey left, FunctionDictionaryKey right)
    {
        return !(left == right);
    }
}
