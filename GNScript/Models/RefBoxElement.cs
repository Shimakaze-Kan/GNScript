using GNScript.Helpers;

namespace GNScript.Models;
public class RefBoxElement
{
    public int ScopeLevel { get; set; }
    public AccessModifier Modifier { get; private set; }
    public RefBoxElementType Type { get; private set; }
    public object? Value { get; private set; }
    public FunctionNode Function { get; private set; }
    public VariableCollection Variables { get; set; } = new();
    public Stack<CallReturnValue> CallReturnValue { get; set; } = new();

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

    public static RefBoxElement CreateFunctionElement(FunctionNode functionNode, AccessModifier accessModifier)
    {
        return new()
        {
            Type = RefBoxElementType.Function,
            Function = functionNode,
            Modifier = accessModifier
        };
    }
}

public enum RefBoxElementType
{
    Field,
    Function
}
