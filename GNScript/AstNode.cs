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

public class StructNode : AstNode
{
    public string Name { get; }
    public List<VariableDeclarationNode> Fields { get; }

    public StructNode(string name, List<VariableDeclarationNode> fields)
    {
        Name = name;
        Fields = fields;
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

public class StructInstanceNode : AstNode
{
    public string StructName { get; }
    public string InstanceName { get; }

    public StructInstanceNode(string structName, string instanceName)
    {
        StructName = structName;
        InstanceName = instanceName;
    }
}

public class StructFieldAccessNode : AstNode
{
    public string InstanceName { get; }
    public string FieldName { get; }

    public StructFieldAccessNode(string instanceName, string fieldName)
    {
        InstanceName = instanceName;
        FieldName = fieldName;
    }
}

public class StructFieldAssignmentNode : AstNode
{
    public string InstanceName { get; }
    public string FieldName { get; }
    public AstNode Value { get; }

    public StructFieldAssignmentNode(string instanceName, string fieldName, AstNode value)
    {
        InstanceName = instanceName;
        FieldName = fieldName;
        Value = value;
    }
}
