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