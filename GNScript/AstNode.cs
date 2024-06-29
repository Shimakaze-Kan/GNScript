using GNScript.Helpers;
using GNScript.Models;

namespace GNScript;
public abstract class AstNode
{
    public AstNode Next { get; set; }
}

public class NumberNode : AstNode
{
    public string Value { get; }

    public NumberNode(string value)
    {
        Value = value;
    }
}

public class BinaryOperationNode : AstNode
{
    public AstNode Left { get; }
    public Token Operator { get; }
    public AstNode Right { get; }

    public BinaryOperationNode(AstNode left, Token op, AstNode right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }
}

public class PrintNode : AstNode
{
    public AstNode Expression { get; }

    public PrintNode(AstNode expression)
    {
        Expression = expression;
    }
}

public class PrintInlineNode : PrintNode
{
    public PrintInlineNode(AstNode expression) : base(expression)
    {
    }
}

public class VariableNode : AstNode
{
    public string Name { get; }

    public VariableNode(string name)
    {
        Name = name;
    }
}

public class AssignmentNode : AstNode
{
    public string Variable { get; }
    public AstNode Expression { get; }

    public AssignmentNode(string variable, AstNode expression)
    {
        Variable = variable;
        Expression = expression;
    }
}

public class IfNode : AstNode
{
    public AstNode Condition { get; }
    public AstNode ThenBranch { get; }
    public AstNode ElseBranch { get; }

    public IfNode(AstNode condition, AstNode thenBranch, AstNode elseBranch)
    {
        Condition = condition;
        ThenBranch = thenBranch;
        ElseBranch = elseBranch;
    }
}

public class FunctionNode : AstNode
{
    public string Name { get; }
    public List<string> Parameters { get; }
    public AstNode Body { get; }

    public FunctionNode(string name, List<string> parameters, AstNode body)
    {
        Name = name;
        Parameters = parameters;
        Body = body;
    }
}

public class FunctionCallNode : AstNode
{
    public string Name { get; }
    public List<AstNode> Arguments { get; }

    public FunctionCallNode(string name, List<AstNode> arguments)
    {
        Name = name;
        Arguments = arguments;
    }
}

public class ReturnNode : AstNode
{
    public AstNode Expression { get; }
    public bool IsVoid { get; }

    public ReturnNode(AstNode expression)
    {
        Expression = expression;
    }

    public ReturnNode(bool isVoid)
    {
        IsVoid = isVoid;
    }
}

public class StringNode : AstNode
{
    public string Value { get; }

    public StringNode(string value)
    {
        Value = value;
    }
}

public class WhileNode : AstNode
{
    public AstNode Condition { get; }
    public AstNode Body { get; }

    public WhileNode(AstNode condition, AstNode body)
    {
        Condition = condition;
        Body = body;
    }
}

public class ForNode : AstNode
{
    public AstNode Init { get; }
    public AstNode Condition { get; }
    public AstNode Increment { get; }
    public AstNode Body { get; }

    public ForNode(AstNode init, AstNode condition, AstNode increment, AstNode body)
    {
        Init = init;
        Condition = condition;
        Increment = increment;
        Body = body;
    }
}

public class InputNode : AstNode
{
}

public class ArrayNode : AstNode
{
    public static string[] Properties => EnumHelpers.GetEnumNamesLowercase<ArrayProperty>();
    public List<AstNode> Elements { get; }

    public ArrayNode(List<AstNode> elements)
    {
        Elements = elements;
    }
}

public class ArrayAccessNode : AstNode
{
    public AstNode Array { get; }
    public AstNode Index { get; }

    public ArrayAccessNode(AstNode array, AstNode index)
    {
        Array = array;
        Index = index;
    }
}

public class ArrayAddNode : AstNode
{
    public AstNode Array { get; }
    public AstNode Element { get; }
    public AstNode Index { get; }

    public ArrayAddNode(AstNode array, AstNode element, AstNode index = null)
    {
        Array = array;
        Element = element;
        Index = index;
    }
}

public class ArrayRemoveNode : AstNode
{
    public AstNode Array { get; }
    public AstNode Index { get; }

    public ArrayRemoveNode(AstNode array, AstNode index)
    {
        Array = array;
        Index = index;
    }
}

public class PropertyAccessNode : AstNode
{
    public AstNode Node { get; }
    public string PropertyName { get; }
    public List<AstNode> Arguments { get; }

    public PropertyAccessNode(AstNode node, string propertyName, List<AstNode> arguments)
    {
        Node = node;
        PropertyName = propertyName;
        Arguments = arguments;
    }
}

public class RefBoxNode : AstNode
{
    public string Name { get; }
    public List<RefBoxAccessModifier<AssignmentNode>> Fields { get; }
    public List<RefBoxAccessModifier<FunctionNode>> Functions { get; }
    public bool IsAbstract { get; }
    public string BaseClassName { get; }

    public RefBoxNode(string name,
        bool isAbstract,
        List<RefBoxAccessModifier<AssignmentNode>> fields,
        List<RefBoxAccessModifier<FunctionNode>> functions,
        string baseClassName)
    {
        Name = name;
        Fields = fields;
        Functions = functions;
        IsAbstract = isAbstract;
        BaseClassName = baseClassName;
    }
}

public class RefBoxAccessModifier<T> : AstNode
{
    public bool IsAbstract { get; }
    public AccessModifier Modifier { get; }
    public T Element { get; }

    public RefBoxAccessModifier(T element, AccessModifier modifier, bool isAbstract)
    {
        Element = element;
        Modifier = modifier;
        IsAbstract = isAbstract;
    }
}

public class VariableDeclarationNode : AstNode
{
    public string Name { get; }
    public AstNode InitialValue { get; }

    public VariableDeclarationNode(string name, AstNode initialValue = null)
    {
        Name = name;
        InitialValue = initialValue;
    }
}

public class RefBoxInstanceNode : AstNode
{
    public string RefBoxName { get; }

    public RefBoxInstanceNode(string refBoxName)
    {
        RefBoxName = refBoxName;
    }
}

public class RefBoxFieldAccessNode : AstNode
{
    public string InstanceName { get; }
    public string FieldName { get; }

    public RefBoxFieldAccessNode(string instanceName, string fieldName)
    {
        InstanceName = instanceName;
        FieldName = fieldName;
    }
}

public class RefBoxFunctionCallNode : AstNode
{
    public string InstanceName { get; private set; }
    public FunctionCallNode FunctionCallNode { get; private set; }

    public RefBoxInstanceNode AnonymousRefBoxInstanceNode { get; set; }
    public RefBoxFunctionCallNode PreviousRefBoxFunction { get; private set; }

    public static RefBoxFunctionCallNode CreateVariableFunctionCall(string instanceName, FunctionCallNode functionCallNode)
    {
        return new RefBoxFunctionCallNode()
        {
            InstanceName = instanceName,
            FunctionCallNode = functionCallNode,
        };
    }

    public static RefBoxFunctionCallNode CreateAnonymousInstanceFunctionCall(RefBoxInstanceNode refBoxInstanceNode, FunctionCallNode functionCallNode)
    {
        return new RefBoxFunctionCallNode()
        {
            AnonymousRefBoxInstanceNode = refBoxInstanceNode,
            FunctionCallNode = functionCallNode,
        };
    }

    public static RefBoxFunctionCallNode CreatePreviousCallWithFunctionCall(RefBoxFunctionCallNode refBoxFunctionCallNode, FunctionCallNode functionCallNode)
    {
        return new RefBoxFunctionCallNode()
            {
            PreviousRefBoxFunction = refBoxFunctionCallNode,
            FunctionCallNode =functionCallNode
                };
    }
}

public class RefBoxFieldAssignmentNode : AstNode
{
    public string InstanceName { get; }
    public string FieldName { get; }
    public AstNode Value { get; }

    public RefBoxFieldAssignmentNode(string instanceName, string fieldName, AstNode value)
    {
        InstanceName = instanceName;
        FieldName = fieldName;
        Value = value;
    }
}

public class ThrowNode : AstNode
{
    public AstNode Message { get; }

    public ThrowNode(AstNode message)
    {
        Message = message;
    }
}

public class ImportNode : AstNode
{
    public AstNode Path { get; }

    public ImportNode(AstNode path)
    {
        Path = path;
    }
}

public class AnonymousValue : AstNode
{
    public object Value { get; }

    public AnonymousValue(object value)
    {
        Value = value;
    }
}
