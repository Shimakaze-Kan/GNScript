using System.Text;

using GNScript.Helpers;
using GNScript.Models;

namespace GNScript;
public class Interpreter
{
    private readonly VariableCollection _variables = new();
    private readonly Dictionary<string, FunctionNode> _functions = [];
    private readonly Stack<CallReturnValue> _callReturnValue = [];
    private int _scopeLevel = 0;
    private bool _isForLoopParameterSection = false;

    public void Run(AstNode node)
    {
        while (node != null)
        {
            Visit(node);
            node = node.Next;
        }
    }

    public ExecutionModel Visit(AstNode node)
    {
        if (node is NumberNode numberNode)
        {
            return int.Parse(numberNode.Value);
        }
        else if (node is StringNode stringNode)
        {
            return stringNode.Value;
        }
        else if (node is VariableNode variableNode)
        {
            return ExecutionModel.FromObject(_variables.GetVariable(variableNode.Name, _scopeLevel));
        }
        else if (node is AssignmentNode assignmentNode)
        {
            var value = Visit(assignmentNode.Expression);
            _variables.SetVariable(assignmentNode.Variable, value.Value, _scopeLevel, _isForLoopParameterSection);
            return ExecutionModel.Empty;
        }
        else if (node is BinaryOperationNode binaryNode)
        {
            var leftValue = Visit(binaryNode.Left);
            var rightValue = Visit(binaryNode.Right);

            if (leftValue.IsEmptyValue || rightValue.IsEmptyValue)
                throw new Exception("Binary operation require value");

            if (leftValue.IsInt() && rightValue.IsInt())
            {
                var leftValueInt = (int)leftValue;
                var rightValueInt = (int)rightValue;
                switch (binaryNode.Operator.Type)
                {
                    case TokenType.Plus:
                        return leftValueInt + rightValueInt;
                    case TokenType.Minus:
                        return leftValueInt - rightValueInt;
                    case TokenType.Multiply:
                        return leftValueInt * rightValueInt;
                    case TokenType.Divide:
                        return leftValueInt / rightValueInt;
                    case TokenType.Modulo:
                        return leftValueInt % rightValueInt;
                    case TokenType.AndOperator:
                        return leftValueInt != 0 && rightValueInt != 0 ? 1 : 0;
                    case TokenType.OrOperator:
                        return leftValueInt != 0 || rightValueInt != 0 ? 1 : 0;
                    case TokenType.Power:
                        return (int)Math.Pow(leftValueInt, rightValueInt);
                    case TokenType.GreaterThan:
                        return leftValueInt > rightValueInt ? 1 : 0;
                    case TokenType.GreaterThanOrEqual:
                        return leftValueInt >= rightValueInt ? 1 : 0;
                    case TokenType.LessThan:
                        return leftValueInt < rightValueInt ? 1 : 0;
                    case TokenType.LessThanOrEqual:
                        return leftValueInt <= rightValueInt ? 1 : 0;
                    case TokenType.Equal:
                        return leftValueInt == rightValueInt ? 1 : 0;
                    case TokenType.NotEqual:
                        return leftValueInt != rightValueInt ? 1 : 0;
                    default:
                        throw new Exception($"Nieznany operator: {binaryNode.Operator.Type}");
                }
            }
            else if (leftValue.IsInt() && rightValue.IsString())
            {
                var leftValueInt = (int)leftValue;
                var rightValueString = (string)rightValue;
                switch (binaryNode.Operator.Type)
                {
                    case TokenType.Plus:
                        return leftValueInt + rightValueString;
                    case TokenType.Multiply:
                        return string.Join("", Enumerable.Repeat(rightValueString, leftValueInt));
                    case TokenType.GreaterThan:
                        return leftValueInt > rightValueString.Length ? 1 : 0;
                    case TokenType.Equal:
                        return leftValueInt == rightValueString.Length ? 1 : 0;
                    case TokenType.NotEqual:
                        return leftValueInt != rightValueString.Length ? 1 : 0;
                    default:
                        throw new Exception($"Nieznany operator: {binaryNode.Operator.Type}");
                }
            }
            else if (leftValue.IsString() && rightValue.IsInt())
            {
                var leftValueString = (string)leftValue;
                var rightValueInt = (int)rightValue;
                switch (binaryNode.Operator.Type)
                {
                    case TokenType.Plus:
                        return leftValueString + rightValueInt;
                    case TokenType.Minus:
                        return leftValueString[..^rightValueInt];
                    case TokenType.Multiply:
                        return string.Join("", Enumerable.Repeat(leftValueString, rightValueInt));
                    case TokenType.Divide:
                        return leftValueString[..(leftValueString.Length / rightValueInt)];
                    case TokenType.LessThan:
                        return leftValueString.Length < rightValueInt ? 1 : 0;
                    case TokenType.LessThanOrEqual:
                        return leftValueString.Length <= rightValueInt ? 1 : 0;
                    case TokenType.Equal:
                        return leftValueString.Length == rightValueInt ? 1 : 0;
                    case TokenType.NotEqual:
                        return leftValueString.Length != rightValueInt ? 1 : 0;
                    default:
                        throw new Exception($"Nieznany operator: {binaryNode.Operator.Type}");
                }
            }
            else
            {
                var leftValueString = (string)leftValue;
                var rightValueString = (string)rightValue;
                switch (binaryNode.Operator.Type)
                {
                    case TokenType.Plus:
                        return leftValueString + rightValueString;
                    case TokenType.Minus:
                        return leftValueString.TrimEnd(rightValueString);
                    case TokenType.Divide:
                        return leftValueString.CountOccurrences(rightValueString);
                    case TokenType.GreaterThan:
                        return leftValueString.CompareTo(rightValueString) == 1 ? 1 : 0;
                    case TokenType.GreaterThanOrEqual:
                        {
                            var comparsionResult = leftValueString.CompareTo(rightValueString);
                            return comparsionResult == 1 || comparsionResult == 0 ? 1 : 0;
                        }
                    case TokenType.LessThan:
                        return leftValueString.CompareTo(rightValueString) == -1 ? 1 : 0;
                    case TokenType.LessThanOrEqual:
                        {
                            var comparsionResult = leftValueString.CompareTo(rightValueString);
                            return comparsionResult == -1 || comparsionResult == 0 ? 1 : 0;
                        }
                    case TokenType.Equal:
                        return leftValueString.CompareTo(rightValueString) == 0 ? 1 : 0;
                    case TokenType.NotEqual:
                        return leftValueString.CompareTo(rightValueString) != 0 ? 1 : 0;
                    default:
                        throw new Exception($"Nieznany operator: {binaryNode.Operator.Type}");
                }
            }
        }
        else if (node is PrintInlineNode printInlineNode)
        {
            var model = Visit(printInlineNode.Expression);
            var value = model.Value;

            if (model.IsArray())
            {
                value = model.ToPrintableArray();
            }
            Console.Write(value);
            return ExecutionModel.Empty;
        }
        else if (node is PrintNode printNode)
        {
            var model = Visit(printNode.Expression);
            var value = model.Value;

            if (model.IsArray())
            {
                value = model.ToPrintableArray();
            }
            Console.WriteLine(value);
            return ExecutionModel.Empty;
        }
        else if (node is FunctionNode functionNode)
        {
            _functions[functionNode.Name] = functionNode;
            return ExecutionModel.Empty;
        }
        else if (node is FunctionCallNode functionCallNode)
        {
            if (_functions.TryGetValue(functionCallNode.Name, out FunctionNode function))
            {
                var callScopeLevel = _scopeLevel;
                _scopeLevel++;

                DeclareFunctionParameters(functionCallNode, function);

                var body = function.Body;

                while (body != null)
                {
                    Visit(body);
                    body = body.Next;

                    if (_callReturnValue.Any())
                    {
                        _variables.ClearScope(callScopeLevel + 1);
                        _scopeLevel = callScopeLevel;
                        var callReturnValue = _callReturnValue.Pop();

                        if (callReturnValue.IsVoid)
                        {
                            return ExecutionModel.Empty;
                        }

                        var returnValue = callReturnValue.ReturnValue;
                        return ExecutionModel.FromObject(returnValue);
                    }
                }

                throw new Exception("No return statement in function");
            }
            else
            {
                throw new Exception($"Invalid function name: '{functionCallNode.Name}'");
            }
        }
        else if (node is IfNode ifNode)
        {
            using (CreateBlockScope())
            {
                var conditionValue = (int)Visit(ifNode.Condition);
                if (conditionValue != 0)
                {
                    var bodyNode = ifNode.ThenBranch;
                    if (ExecuteBody(bodyNode))
                        return ExecutionModel.Empty;
                }
                else if (ifNode.ElseBranch != null)
                {
                    var bodyNode = ifNode.ElseBranch;
                    if (ExecuteBody(bodyNode))
                        return ExecutionModel.Empty;
                }
            }

            return ExecutionModel.Empty;
        }
        else if (node is WhileNode whileNode)
        {
            using (CreateBlockScope())
            {
                while ((int)Visit(whileNode.Condition) != 0)
                {
                    var bodyNode = whileNode.Body;
                    if (ExecuteBody(bodyNode))
                        return ExecutionModel.Empty;
                }
            }

            return ExecutionModel.Empty;
        }
        else if (node is ForNode forNode)
        {
            using (CreateBlockScope())
            {
                using (CreateForLoopParameterScope())
                {
                    Visit(forNode.Init);
                }
                for (; (int)Visit(forNode.Condition) != 0;)
                {
                    var bodyNode = forNode.Body;
                    if (ExecuteBody(bodyNode))
                        return ExecutionModel.Empty;

                    using (CreateForLoopParameterScope())
                    {
                        Visit(forNode.Increment);
                    }
                }
            }

            return ExecutionModel.Empty;
        }
        else if (node is ReturnNode returnNode)
        {
            if (returnNode.IsVoid)
            {
                _callReturnValue.Push(CallReturnValue.CreateVoidReturnValue());
                return ExecutionModel.Empty;
            }

            var returnValue = Visit(returnNode.Expression).Value;
            _callReturnValue.Push(new CallReturnValue(returnValue));
            return ExecutionModel.Empty;
        }
        else if (node is InputNode)
        {
            var input = Console.ReadLine() ?? "";
            if (int.TryParse(input, out var value))
            {
                return value;
            }

            return input;
        }
        else if (node is ArrayAccessNode arrayAccessNode)
        {
            var model = Visit(arrayAccessNode.Array);

            if (model.IsArray() == false)
            {
                throw new InvalidOperationException("Cannot access array");
            }

            var arrayValue = (List<object>)model;
            var indexModel = Visit(arrayAccessNode.Index);
            if (indexModel.IsInt() == false)
            {
                throw new InvalidOperationException("Array index should be a number");
            }

            return ExecutionModel.FromObject(arrayValue[(int)indexModel]);
        }
        else if (node is ArrayNode arrayNode)
        {
            var elements = arrayNode.Elements.Select(Visit).Select(x => x.Value).ToList();
            return ExecutionModel.FromObject(elements);
        }
        else if (node is PropertyAccessNode propertyNode)
        {
            var nodeModel = Visit(propertyNode.Node);

            if (nodeModel.IsEmptyValue)
                return ExecutionModel.Empty;

            var commonProperties = EnumHelpers.GetEnumNamesLowercase<CommonValueProperty>();
            if (commonProperties.Contains(propertyNode.PropertyName))
            {
                if (EnumHelpers.EqualsIgnoreCase(propertyNode.PropertyName, CommonValueProperty.Type))
                {
                    return Enum.GetName(typeof(ExecutionModelValueType), nodeModel.ModelType);
                }
            }

            if (nodeModel.IsArray() && ArrayNode.Properties.Contains(propertyNode.PropertyName))
            {
                var originalArrayValue = (List<object>)nodeModel;
                var arrayValue = originalArrayValue.DeepCopy(); // GN Script array is not reference type by language convention

                if (EnumHelpers.EqualsIgnoreCase(propertyNode.PropertyName, ArrayProperty.Length))
                {
                    return arrayValue.Count;
                }
                else if (EnumHelpers.EqualsIgnoreCase(propertyNode.PropertyName, ArrayProperty.Reverse))
                {
                    arrayValue.Reverse();
                    return ExecutionModel.FromObject(arrayValue);
                }
                else if (EnumHelpers.EqualsIgnoreCase(propertyNode.PropertyName, ArrayProperty.ToString))
                {
                    return string.Join("", arrayValue);
                }
                else if (EnumHelpers.EqualsIgnoreCase(propertyNode.PropertyName, ArrayProperty.RemoveAt))
                {
                    ExceptionsHelper.FailIfTrue(propertyNode.Arguments.Count != 1, "Expected 1 argument");
                    var atModel = Visit(propertyNode.Arguments[0]);

                    ExceptionsHelper.FailIfTrue(atModel.IsEmptyValue, "Expected value argument");
                    ExceptionsHelper.FailIfFalse(atModel.IsInt(), "Expected Int argument");

                    arrayValue.RemoveAt((int)atModel);
                    return ExecutionModel.FromObject(arrayValue);
                }
                else if (EnumHelpers.EqualsIgnoreCase(propertyNode.PropertyName, ArrayProperty.Append))
                {
                    ExceptionsHelper.FailIfTrue(propertyNode.Arguments.Count != 1, "Expected 1 arguments");
                    var valueModel = Visit(propertyNode.Arguments[0]);

                    arrayValue.Add(valueModel.Value);
                    return ExecutionModel.FromObject(arrayValue);
                }
                else if (EnumHelpers.EqualsIgnoreCase(propertyNode.PropertyName, ArrayProperty.AddAt))
                {
                    ExceptionsHelper.FailIfTrue(propertyNode.Arguments.Count != 2, "Expected 2 arguments");
                    var atModel = Visit(propertyNode.Arguments[0]);
                    var valueModel = Visit(propertyNode.Arguments[1]);

                    ExceptionsHelper.FailIfFalse(atModel.IsInt(), "Expected Int argument");
                    arrayValue.Insert((int)atModel, valueModel.Value);
                    return ExecutionModel.FromObject(arrayValue);
                }
                else if (EnumHelpers.EqualsIgnoreCase(propertyNode.PropertyName, ArrayProperty.Prepend))
                {
                    ExceptionsHelper.FailIfTrue(propertyNode.Arguments.Count != 1, "Expected 1 arguments");
                    var valueModel = Visit(propertyNode.Arguments[0]);

                    arrayValue.Insert(0, valueModel.Value);
                    return ExecutionModel.FromObject(arrayValue);
                }
                else if (EnumHelpers.EqualsIgnoreCase(propertyNode.PropertyName, ArrayProperty.ReplaceAt))
                {
                    ExceptionsHelper.FailIfTrue(propertyNode.Arguments.Count < 2, "Expected at least 2 arguments");

                    List<object> parameters = [];
                    for (int i = 0; i < propertyNode.Arguments.Count - 1; i++)
                    {
                        var indexModel = Visit(propertyNode.Arguments[i]);
                        ExceptionsHelper.FailIfFalse(indexModel.IsInt(), "Expected Int argument");

                        parameters.Add((int)indexModel);
                    }

                    var valueModel = Visit(propertyNode.Arguments[^1]);
                    parameters.Add(valueModel.Value);
                    arrayValue.ReplaceGNArray([.. parameters]);

                    return ExecutionModel.FromObject(arrayValue);
                }
            }

            var stringProperties = EnumHelpers.GetEnumNamesLowercase<StringProperty>();
            if (nodeModel.IsString() && stringProperties.Contains(propertyNode.PropertyName))
            {
                var stringValue = (string)nodeModel;
                if (EnumHelpers.EqualsIgnoreCase(propertyNode.PropertyName, StringProperty.ToLower))
                {
                    return stringValue.ToLower();
                }
                else if (EnumHelpers.EqualsIgnoreCase(propertyNode.PropertyName, StringProperty.ToUpper))
                {
                    return stringValue.ToUpper();
                }
                else if (EnumHelpers.EqualsIgnoreCase(propertyNode.PropertyName, StringProperty.Reverse))
                {
                    return string.Join("", stringValue.Reverse());
                }
                else if (EnumHelpers.EqualsIgnoreCase(propertyNode.PropertyName, StringProperty.ToArray))
                {
                    return ExecutionModel.FromObject(stringValue.ToList());
                }
            }

            throw new Exception($"Property '{propertyNode.PropertyName}' not found");
        }

        throw new Exception("AST node error");
    }

    public void Dump()
    {
        var sb = new StringBuilder();

        sb.AppendLine("[Variables]");
        var variablesDump = _variables.ToString();

        if (string.IsNullOrEmpty(variablesDump))
            sb.AppendLine("  No variables to display.\n");
        else
            sb.AppendLine(_variables.ToString());

        sb.AppendLine("[Functions]");
        foreach (var (name, function) in _functions)
        {
            sb.AppendLine($"  {name} <- {{{string.Join(", ", function.Parameters)}}}");
        }

        if (_functions.Count == 0)
            sb.AppendLine("  No functions to display.");

        Console.WriteLine(sb);
    }

    private void DeclareFunctionParameters(FunctionCallNode functionCallNode, FunctionNode function)
    {
        for (int i = 0; i < function.Parameters.Count; i++)
        {
            _variables.SetVariable(function.Parameters[i], Visit(functionCallNode.Arguments[i]).Value, _scopeLevel, true);
        }
    }

    private bool ExecuteBody(AstNode bodyNode)
    {
        var hasCallReturnValues = _callReturnValue.Any();

        while (bodyNode != null && hasCallReturnValues == false)
        {
            Visit(bodyNode);
            bodyNode = bodyNode.Next;
        }

        return hasCallReturnValues;
    }

    private BodyHandler CreateBlockScope()
    {
        return new BodyHandler(() => _scopeLevel++, () =>
        {
            _variables.ClearScope(_scopeLevel);
            _scopeLevel--;
        });
    }

    private BodyHandler CreateForLoopParameterScope()
    {
        return new BodyHandler(() => _isForLoopParameterSection = true, () => _isForLoopParameterSection = false);
    }
}