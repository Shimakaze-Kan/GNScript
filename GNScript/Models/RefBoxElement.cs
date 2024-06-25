namespace GNScript.Models;
public class RefBoxElement
{
    public AccessModifier Modifier { get; private set; }
    public RefBoxElementType Type { get; private set; }
    public object? Value { get; private set; }
    public string? FunctionName { get; private set; }

    private RefBoxElement()
    { }

    public static RefBoxElement CreateFieldElement(object value, AccessModifier accessModifier)
    {
        return new()
        {
            Type = RefBoxElementType.Field,
            Value = value,
            Modifier = accessModifier
        };
    }

    public static RefBoxElement CreateFunctionElement(string functionName, AccessModifier accessModifier)
    {
        return new()
        {
            Type = RefBoxElementType.Function,
            FunctionName = functionName,
            Modifier = accessModifier
        };
    }
}

public enum RefBoxElementType
{
    Field,
    Function
}
