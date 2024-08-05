using GNScript.Exceptions;
using GNScript.Helpers;
using GNScript.Models;
using System.Text;

namespace GNScript;
public class Interpreter
{
    private VariableCollection _variables = new();
    private Dictionary<FunctionDictionaryKey, FunctionNode> _functions = [];
    private Dictionary<string, RefBoxNode> _refBoxDefinitions = [];
    private Stack<CallReturnValue> _callReturnValue = [];
    private Dictionary<string, UserDefinedExtension> _userDefinedExtensions = [];
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
                        throw new Exception($"Unknown operator: {binaryNode.Operator.Type}");
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
                        throw new Exception($"Unknown operator: {binaryNode.Operator.Type}");
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
                        throw new Exception($"Unknown operator: {binaryNode.Operator.Type}");
                }
            }
            else if (leftValue.IsArray() && rightValue.IsInt())
            {
                var leftValueArray = (List<object>)leftValue;
                var rightValueInt = (int)rightValue;
                switch (binaryNode.Operator.Type)
                {
                    case TokenType.Plus:
                        leftValueArray.Add(rightValueInt);
                        return ExecutionModel.FromObject(leftValueArray);
                    case TokenType.Minus:
                        var trimmedArray = leftValueArray[0..^rightValueInt];
                        return ExecutionModel.FromObject(trimmedArray);
                    case TokenType.Divide:
                        var chunks = leftValueArray.Chunk(rightValueInt);
                        var list = new List<object>();

                        foreach (var chunk in chunks)
                        {
                            list.Add(chunk.ToList());
                        }

                        return ExecutionModel.FromObject(list);
                    case TokenType.Multiply:
                        return ExecutionModel.FromObject(leftValueArray.RepeatList(rightValueInt));
                    case TokenType.LessThan:
                        return leftValueArray.Count < rightValueInt ? 1 : 0;
                    case TokenType.LessThanOrEqual:
                        return leftValueArray.Count <= rightValueInt ? 1 : 0;
                    case TokenType.Equal:
                        return leftValueArray.Count == rightValueInt ? 1 : 0;
                    case TokenType.NotEqual:
                        return leftValueArray.Count != rightValueInt ? 1 : 0;
                    default:
                        throw new Exception($"Unknown operator: {binaryNode.Operator.Type}");
                }
            }
            else if (leftValue.IsInt() && rightValue.IsArray())
            {
                var leftValueInt = (int)leftValue;
                var rightValueArray = (List<object>)rightValue;
                switch (binaryNode.Operator.Type)
                {
                    case TokenType.Plus:
                        var extenedArray = rightValueArray.Prepend(leftValueInt).ToList();
                        return ExecutionModel.FromObject(extenedArray);
                    default:
                        throw new Exception($"Unknown operator: {binaryNode.Operator.Type}");
                }
            }
            else if (leftValue.IsString() && rightValue.IsArray())
            {
                var leftValueString = (string)leftValue;
                var rightValueArray = (List<object>)rightValue;
                switch (binaryNode.Operator.Type)
                {
                    case TokenType.Plus:
                        var extenedArray = rightValueArray.Prepend(leftValueString).ToList();
                        return ExecutionModel.FromObject(extenedArray);
                    default:
                        throw new Exception($"Unknown operator: {binaryNode.Operator.Type}");
                }
            }
            else if (leftValue.IsArray() && rightValue.IsString())
            {
                var leftValueArray = (List<object>)leftValue;
                var rightValueString = (string)rightValue;
                switch (binaryNode.Operator.Type)
                {
                    case TokenType.Plus:
                        var extenedArray = leftValueArray.Append(rightValueString).ToList();
                        return ExecutionModel.FromObject(extenedArray);
                    default:
                        throw new Exception($"Unknown operator: {binaryNode.Operator.Type}");
                }
            }
            else if (leftValue.IsArray() && rightValue.IsArray())
            {
                var leftValueArray = (List<object>)leftValue;
                var rightValueArray = (List<object>)rightValue;
                switch (binaryNode.Operator.Type)
                {
                    case TokenType.Plus:
                        leftValueArray.AddRange(rightValueArray);
                        return ExecutionModel.FromObject(leftValueArray);
                    case TokenType.Minus:
                        if (rightValueArray.Count <= leftValueArray.Count)
                        {
                            bool endsMatch = true;
                            for (int i = 0; i < rightValueArray.Count; i++)
                            {
                                if (!rightValueArray[rightValueArray.Count - 1 - i].Equals(leftValueArray[leftValueArray.Count - 1 - i]))
                                {
                                    endsMatch = false;
                                    break;
                                }
                            }

                            if (endsMatch)
                            {
                                return ExecutionModel.FromObject(leftValueArray.Take(leftValueArray.Count - rightValueArray.Count).ToList());
                            }
                        }

                        return ExecutionModel.FromObject(leftValueArray);
                    case TokenType.Divide:
                        return ExecutionModel.FromObject(leftValueArray.CountOccurrences(rightValueArray));
                    case TokenType.Multiply:
                        var areAllRightArrayElementsAnInt = rightValueArray.All(x => x is int);
                        if (areAllRightArrayElementsAnInt == false)
                        {
                            throw new Exception("Right array must consist of int only");
                        }
                        return ExecutionModel.FromObject(leftValueArray.MultiplyLists(rightValueArray.Cast<int>().ToList()));
                    case TokenType.LessThan:
                        return leftValueArray.Count < rightValueArray.Count ? 1 : 0;
                    case TokenType.GreaterThan:
                        return leftValueArray.Count > rightValueArray.Count ? 1 : 0;
                    case TokenType.Equal:
                        return Enumerable.SequenceEqual(leftValueArray, rightValueArray) ? 1 : 0;
                    case TokenType.NotEqual:
                        return Enumerable.SequenceEqual(leftValueArray, rightValueArray) ? 0 : 1;
                    default:
                        throw new Exception($"Unknown operator: {binaryNode.Operator.Type}");
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
                        throw new Exception($"Unknown operator: {binaryNode.Operator.Type}");
                }
            }
        }
        else if (node is PrintInlineNode printInlineNode)
        {
            var model = Visit(printInlineNode.Expression);

            if (model.IsString())
                Console.Write((string)model);
            else
                Console.Write(model.ToPrintable());

            return ExecutionModel.Empty;
        }
        else if (node is PrintNode printNode)
        {
            var model = Visit(printNode.Expression);

            if (model.IsString())
                Console.WriteLine((string)model);
            else
                Console.WriteLine(model.ToPrintable());

            return ExecutionModel.Empty;
        }
        else if (node is FunctionNode functionNode)
        {
            _functions[new(functionNode)] = functionNode;
            return ExecutionModel.Empty;
        }
        else if (node is FunctionCallNode functionCallNode)
        {
            if (_functions.TryGetValue(new(functionCallNode), out FunctionNode function))
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
        else if (node is ExtensionAccessNode extensionNode)
        {
            var nodeModel = Visit(extensionNode.Node);

            if (nodeModel.IsEmptyValue)
                return ExecutionModel.Empty;

            var commonExtensions = EnumHelpers.GetEnumNamesLowercase<CommonValueExtension>();
            if (commonExtensions.Contains(extensionNode.ExtensionName))
            {
                if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, CommonValueExtension.Type))
                {
                    return Enum.GetName(typeof(ExecutionModelValueType), nodeModel.ModelType);
                }
            }

            if (nodeModel.IsArray())
            {
                var originalArrayValue = (List<object>)nodeModel;
                var arrayValue = originalArrayValue.DeepCopy(); // GN Script array is not reference type by language convention

                if (_userDefinedExtensions.TryGetValue(extensionNode.ExtensionName.ToLower(), out var userDefinedExtension))
                {
                    if (userDefinedExtension.Type == "array")
                    {
                        ExceptionsHelper.FailIfFalse(extensionNode.Arguments.Count == userDefinedExtension.FunctionParameterNames.Count - 1,
                            $"Expected {userDefinedExtension.FunctionParameterNames.Count - 1} parameters");

                        var callScopeLevel = _scopeLevel;
                        _scopeLevel++;

                        // pass value before extension as first parameter
                        _variables.SetVariable(userDefinedExtension.FunctionParameterNames[0], arrayValue, _scopeLevel, true);

                        for (int i = 0; i < extensionNode.Arguments.Count; i++)
                        {
                            _variables.SetVariable(userDefinedExtension.FunctionParameterNames[i + 1], extensionNode.Arguments[i], _scopeLevel, true);
                        }

                        var body = userDefinedExtension.FunctionBody;
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

                        throw new Exception("No return statement in extension referenced function");
                    }
                }

                if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, ArrayExtension.Length))
                {
                    return arrayValue.Count;
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, ArrayExtension.Reverse))
                {
                    arrayValue.Reverse();
                    return ExecutionModel.FromObject(arrayValue);
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, ArrayExtension.ToString))
                {
                    ExceptionsHelper.FailIfTrue(extensionNode.Arguments.Count != 0 && extensionNode.Arguments.Count != 1, "Expected 0 or 1 arguments");

                    if (extensionNode.Arguments.Count == 1)
                    {
                        var connectModel = Visit(extensionNode.Arguments[0]);
                        ExceptionsHelper.FailIfFalse(connectModel.IsString(), "Expected string argument");
                        var connect = (string)connectModel;

                        return string.Join(connect, arrayValue);
                    }

                    return string.Join("", arrayValue);
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, ArrayExtension.RemoveAt))
                {
                    ExceptionsHelper.FailIfTrue(extensionNode.Arguments.Count != 1, "Expected 1 argument");
                    var atModel = Visit(extensionNode.Arguments[0]);

                    ExceptionsHelper.FailIfTrue(atModel.IsEmptyValue, "Expected value argument");
                    ExceptionsHelper.FailIfFalse(atModel.IsInt(), "Expected Int argument");

                    arrayValue.RemoveAt((int)atModel);
                    return ExecutionModel.FromObject(arrayValue);
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, ArrayExtension.Append))
                {
                    ExceptionsHelper.FailIfTrue(extensionNode.Arguments.Count != 1, "Expected 1 arguments");
                    var valueModel = Visit(extensionNode.Arguments[0]);

                    arrayValue.Add(valueModel.Value);
                    return ExecutionModel.FromObject(arrayValue);
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, ArrayExtension.AddAt))
                {
                    ExceptionsHelper.FailIfTrue(extensionNode.Arguments.Count != 2, "Expected 2 arguments");
                    var atModel = Visit(extensionNode.Arguments[0]);
                    var valueModel = Visit(extensionNode.Arguments[1]);

                    ExceptionsHelper.FailIfFalse(atModel.IsInt(), "Expected Int argument");
                    arrayValue.Insert((int)atModel, valueModel.Value);
                    return ExecutionModel.FromObject(arrayValue);
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, ArrayExtension.Prepend))
                {
                    ExceptionsHelper.FailIfTrue(extensionNode.Arguments.Count != 1, "Expected 1 arguments");
                    var valueModel = Visit(extensionNode.Arguments[0]);

                    arrayValue.Insert(0, valueModel.Value);
                    return ExecutionModel.FromObject(arrayValue);
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, ArrayExtension.ReplaceAt))
                {
                    ExceptionsHelper.FailIfTrue(extensionNode.Arguments.Count < 2, "Expected at least 2 arguments");

                    List<object> parameters = [];
                    for (int i = 0; i < extensionNode.Arguments.Count - 1; i++)
                    {
                        var indexModel = Visit(extensionNode.Arguments[i]);
                        ExceptionsHelper.FailIfFalse(indexModel.IsInt(), "Expected Int argument");

                        parameters.Add((int)indexModel);
                    }

                    var valueModel = Visit(extensionNode.Arguments[^1]);
                    parameters.Add(valueModel.Value);
                    arrayValue.ReplaceGNArray([.. parameters]);

                    return ExecutionModel.FromObject(arrayValue);
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, ArrayExtension.Has))
                {
                    ExceptionsHelper.FailIfTrue(extensionNode.Arguments.Count != 1, "Expected 1 arguments");
                    var valueModel = Visit(extensionNode.Arguments[0]);

                    return arrayValue.Contains(valueModel.Value) ? 1 : 0;
                }
            }

            var stringExtensions = EnumHelpers.GetEnumNamesLowercase<StringExtension>();
            if (nodeModel.IsString() && stringExtensions.Contains(extensionNode.ExtensionName))
            {
                var stringValue = (string)nodeModel;
                if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, StringExtension.ToLower))
                {
                    return stringValue.ToLower();
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, StringExtension.ToUpper))
                {
                    return stringValue.ToUpper();
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, StringExtension.Reverse))
                {
                    return string.Join("", stringValue.Reverse());
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, StringExtension.ToArray))
                {
                    return ExecutionModel.FromObject(stringValue.ToList().ConvertAll(c => c.ToString()));
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, StringExtension.Length))
                {
                    return ExecutionModel.FromObject(stringValue.Length);
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, StringExtension.Split))
                {
                    ExceptionsHelper.FailIfTrue(extensionNode.Arguments.Count != 0 && extensionNode.Arguments.Count != 1, "Expected 0 or 1 arguments");

                    if (extensionNode.Arguments.Count == 1)
                    {
                        var valueModel = Visit(extensionNode.Arguments[0]);
                        ExceptionsHelper.FailIfFalse(valueModel.IsString(), "Expected string");
                        var splitValue = (string)valueModel;

                        return ExecutionModel.FromObject(stringValue.Split(splitValue).Cast<object>().ToList());
                    }

                    return ExecutionModel.FromObject(stringValue.Split().Cast<object>().ToList());
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, StringExtension.ReplaceAt))
                {
                    ExceptionsHelper.FailIfTrue(extensionNode.Arguments.Count != 2, "Expected 2 arguments");
                    var indexModel = Visit(extensionNode.Arguments[0]);
                    var valueModel = Visit(extensionNode.Arguments[1]);
                    ExceptionsHelper.FailIfFalse(indexModel.IsInt(), "Expected int index");
                    ExceptionsHelper.FailIfFalse(valueModel.IsString(), "Expected string value");

                    int index = (int)indexModel;
                    string value = (string)valueModel;

                    ExceptionsHelper.FailIfTrue(index < 0 || index >= stringValue.Length, "Index out of range");

                    string result = $"{stringValue[..index]}{value}{stringValue[(index + 1)..]}";
                    return result;
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, StringExtension.ToInt))
                {
                    return int.Parse(stringValue);
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, StringExtension.CanConvertToInt))
                {
                    return int.TryParse(stringValue, out _) ? 1 : 0;
                }
            }

            var refBoxExtensions = EnumHelpers.GetEnumNamesLowercase<BoxExtension>();
            if (nodeModel.IsRefBox() && refBoxExtensions.Contains(extensionNode.ExtensionName))
            {
                if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, BoxExtension.IsInstanceOf))
                {
                    ExceptionsHelper.FailIfTrue(extensionNode.Arguments.Count != 1, "Expected 1 arguments");
                    var valueModel = Visit(extensionNode.Arguments[0]);
                    ExceptionsHelper.FailIfFalse(valueModel.IsString(), "Expected ref box name");
                    var refBoxName = (string)valueModel;

                    var instanceName = ((VariableNode)extensionNode.Node).Name;
                    var refBox = (Dictionary<FunctionVariableDictionaryKey, RefBoxElement>)_variables.GetVariable(instanceName, _scopeLevel);
                    var instanceFields = refBox.Keys.Select(k => k.VariableName).Where(v => string.IsNullOrEmpty(v) == false).ToList();
                    var definitionFields = _refBoxDefinitions[refBoxName].Fields.ConvertAll(field => field.Element.Variable);

                    var instanceFunctions = refBox.Where(v => v.Key.FunctionKey != null).Select(f => f.Key.FunctionKey).ToList();
                    var definitionFunctions = _refBoxDefinitions[refBoxName].Functions.ConvertAll(f => new FunctionDictionaryKey(f.Element));

                    var sameFields = Enumerable.SequenceEqual(definitionFields.Order(), instanceFields.Order());
                    var sameFunctions = Enumerable.SequenceEqual(definitionFunctions.Order(), instanceFunctions.Order());
                    return sameFields && sameFunctions ? 1 : 0;
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, BoxExtension.HasField))
                {
                    ExceptionsHelper.FailIfTrue(extensionNode.Arguments.Count != 1 && extensionNode.Arguments.Count != 2, "Expected 1 or 2 arguments");
                    var valueModel = Visit(extensionNode.Arguments[0]);
                    ExceptionsHelper.FailIfFalse(valueModel.IsString(), "Expected field name");
                    var fieldName = (string)valueModel;
                    var showGuarded = 0;

                    if (extensionNode.Arguments.Count == 2)
                    {
                        valueModel = Visit(extensionNode.Arguments[1]);
                        ExceptionsHelper.FailIfFalse(valueModel.IsInt(), "Expected bool");

                        showGuarded = (int)valueModel;
                    }

                    var instanceName = string.Empty;

                    if (extensionNode.Node is VariableNode variable)
                    {
                        instanceName = variable.Name;
                    }
                    else if (extensionNode.Node is RefBoxFunctionCallNode refBoxFunctionCallNode)
                    {
                        instanceName = refBoxFunctionCallNode.InstanceName;
                        if (string.IsNullOrEmpty(instanceName)) // it means it's anonymous instance
                        {
                            throw new Exception("HasField extension cannot be invoked on anonymous instance");
                        }
                    }

                    var refBox = (Dictionary<FunctionVariableDictionaryKey, RefBoxElement>)_variables.GetVariable(instanceName, _scopeLevel);
                    var instanceFields = refBox.Where(kv => showGuarded == 1 ? true : kv.Value.Modifier == AccessModifier.Exposed).Select(kv => kv.Key).Select(k => k.VariableName);

                    return instanceFields.Contains(fieldName) ? 1 : 0;
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, BoxExtension.HasFunction))
                {
                    ExceptionsHelper.FailIfTrue(extensionNode.Arguments.Count != 2, "Expected 2 arguments");
                    var valueModel = Visit(extensionNode.Arguments[0]);
                    ExceptionsHelper.FailIfFalse(valueModel.IsString(), "Expected function name");
                    var functionName = (string)valueModel;

                    valueModel = Visit(extensionNode.Arguments[1]);
                    ExceptionsHelper.FailIfFalse(valueModel.IsInt(), "Expected number of parameters");
                    var parametersCount = (int)valueModel;

                    var instanceName = ((VariableNode)extensionNode.Node).Name;
                    var refBox = (Dictionary<FunctionVariableDictionaryKey, RefBoxElement>)_variables.GetVariable(instanceName, _scopeLevel);
                    var instanceFuncs = refBox.Keys.Where(k => k.FunctionKey != null).Select(k => k.FunctionKey).ToList();

                    return instanceFuncs.Any(f => f.FunctionName == functionName && f.FunctionParameterCount == parametersCount) ? 1 : 0;
                }
                else if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, BoxExtension.ReflectionSetField))
                {
                    ExceptionsHelper.FailIfTrue(extensionNode.Arguments.Count != 2, "Expected 2 arguments");
                    var valueModel = Visit(extensionNode.Arguments[0]);
                    ExceptionsHelper.FailIfFalse(valueModel.IsString(), "Expected field name");
                    var fieldName = (string)valueModel;

                    valueModel = Visit(extensionNode.Arguments[1]);
                    var newValuemodel = valueModel.Value;

                    var instanceName = ((VariableNode)extensionNode.Node).Name;
                    var refBox = (Dictionary<FunctionVariableDictionaryKey, RefBoxElement>)_variables.GetVariable(instanceName, _scopeLevel);

                    var modifier = refBox[new(fieldName)].Modifier;
                    refBox[new(fieldName)] = RefBoxElement.CreateFieldElement(newValuemodel, modifier);

                    return ExecutionModel.Empty;
                }
            }

            var intExtensions = EnumHelpers.GetEnumNamesLowercase<IntExtension>();
            if (nodeModel.IsInt() && intExtensions.Contains(extensionNode.ExtensionName))
            {
                var intValue = (int)nodeModel;
                if (EnumHelpers.EqualsIgnoreCase(extensionNode.ExtensionName, IntExtension.ToString))
                {
                    return intValue.ToString();
                }
            }

            throw new Exception($"Extension '{extensionNode.ExtensionName}' not found");
        }
        else if (node is RefBoxNode refBoxNode)
        {
            if (_refBoxDefinitions.TryGetValue(refBoxNode.Name, out var existingRefBoxDefinition))
            {
                ExceptionsHelper.FailIfTrue(existingRefBoxDefinition.IsConst, $"Refbox '{existingRefBoxDefinition.Name}' is const, cannot create ref box definition with the same name");
            }

            var fieldNames = refBoxNode.Fields.ConvertAll(f => f.Element.Variable);
            ExceptionsHelper.FailIfFalse(fieldNames.Count == fieldNames.Distinct().Count(), "Field name duplication");

            var funcKeys = refBoxNode.Functions.ConvertAll(f => new FunctionDictionaryKey(f.Element));
            ExceptionsHelper.FailIfFalse(funcKeys.Count == funcKeys.Distinct().Count(), "Func duplication");

            if (string.IsNullOrEmpty(refBoxNode.BaseClassName) == false)
            {
                ExceptionsHelper.FailIfFalse(_refBoxDefinitions.TryGetValue(refBoxNode.BaseClassName, out var baseRefBoxDefinition),
                    $"Base ref box '{refBoxNode.BaseClassName}' not found");

                foreach (var field in baseRefBoxDefinition.Fields)
                {
                    if (refBoxNode.Fields.Any(f => f.Element.Variable == field.Element.Variable))
                        continue;

                    refBoxNode.Fields.Add(field);
                }

                foreach (var func in baseRefBoxDefinition.Functions)
                {
                    if (refBoxNode.Functions.Any(f => new FunctionDictionaryKey(f.Element) == new FunctionDictionaryKey(func.Element)))
                        continue;

                    refBoxNode.Functions.Add(func);
                }
            }

            var notOverridedAbstractFunctions = refBoxNode.Functions.Where(f => f.IsAbstract);
            ExceptionsHelper.FailIfTrue(notOverridedAbstractFunctions.Count() != 0 && refBoxNode.IsAbstract == false,
                $"Refbox cannot have not overrided functions: {string.Join(", ", notOverridedAbstractFunctions.Select(f => f.Element.Name))}");

            _refBoxDefinitions[refBoxNode.Name] = refBoxNode;
            return ExecutionModel.Empty;
        }
        else if (node is RefBoxInstanceNode refBoxInstanceNode)
        {
            var refBoxDefinition = _refBoxDefinitions[refBoxInstanceNode.RefBoxName];
            var instance = new Dictionary<FunctionVariableDictionaryKey, RefBoxElement>();
            ExceptionsHelper.FailIfTrue(refBoxDefinition.IsAbstract, "Cannot make instance of abstract refbox");

            foreach (var field in refBoxDefinition.Fields)
            {
                instance[new(field.Element.Variable)] = RefBoxElement.CreateFieldElement(Visit(field.Element.Expression).Value, field.Modifier);
            }

            foreach (var func in refBoxDefinition.Functions)
            {
                instance[new(func.Element)] = RefBoxElement.CreateFunctionElement(func.Element, func.Modifier);
            }

            return ExecutionModel.FromObject(instance);
        }
        else if (node is RefBoxFieldAccessNode refBoxFieldAccessNode)
        {
            var instance = (Dictionary<FunctionVariableDictionaryKey, RefBoxElement>)_variables.GetVariable(refBoxFieldAccessNode.InstanceName, _scopeLevel);

            if (instance[new(refBoxFieldAccessNode.FieldName)].Modifier == AccessModifier.Guarded)
            {
                throw new Exception("Cannot access guarded field");
            }

            return ExecutionModel.FromObject(instance[new(refBoxFieldAccessNode.FieldName)].Value);
        }
        else if (node is RefBoxFieldAssignmentNode refBoxFieldAssignmentNode)
        {
            var instance = (Dictionary<FunctionVariableDictionaryKey, RefBoxElement>)_variables.GetVariable(refBoxFieldAssignmentNode.InstanceName, _scopeLevel);
            var modifier = instance[new(refBoxFieldAssignmentNode.FieldName)].Modifier;

            if (modifier == AccessModifier.Guarded)
            {
                throw new Exception("Cannot set value to guarded field");
            }

            instance[new(refBoxFieldAssignmentNode.FieldName)] = RefBoxElement.CreateFieldElement(Visit(refBoxFieldAssignmentNode.Value).Value, modifier);
            return ExecutionModel.Empty;
        }
        else if (node is ThrowNode throwNode)
        {
            var message = (string)Visit(throwNode.Message);
            throw new UserDefinedException(message);
        }
        else if (node is RefBoxFunctionCallNode refBoxFunctionCallNode)
        {
            Dictionary<FunctionVariableDictionaryKey, RefBoxElement> instance = null;

            if (string.IsNullOrEmpty(refBoxFunctionCallNode.InstanceName) == false)
            {
                instance = (Dictionary<FunctionVariableDictionaryKey, RefBoxElement>)_variables.GetVariable(refBoxFunctionCallNode.InstanceName, _scopeLevel);
            }
            else if (refBoxFunctionCallNode.AnonymousRefBoxInstanceNode != null)
            {
                instance = (Dictionary<FunctionVariableDictionaryKey, RefBoxElement>)Visit(refBoxFunctionCallNode.AnonymousRefBoxInstanceNode).Value;
            }
            else if (refBoxFunctionCallNode.PreviousRefBoxFunction != null)
            {
                Stack<AstNode> callChain = [];

                AstNode currentNode = refBoxFunctionCallNode;
                while (currentNode != null)
                {
                    callChain.Push(currentNode);
                    currentNode = (currentNode as RefBoxFunctionCallNode)?.PreviousRefBoxFunction;
                }

                var firstCall = callChain.ToList()[0];
                callChain = new Stack<AstNode>(callChain.ToList().Skip(1));

                var firstCallReturnValue = (Dictionary<FunctionVariableDictionaryKey, RefBoxElement>)Visit(firstCall).Value;

                List<string> anonymousRefBoxDefinitionNames = [];

                RefBoxInstanceNode anonymousRefBoxInstance = CreateAnonymousRefBoxInstance(firstCallReturnValue, anonymousRefBoxDefinitionNames);

                var callReturnInstance = anonymousRefBoxInstance;
                object lastCallValue = null;
                foreach (var call in callChain.Reverse())
                {
                    (call as RefBoxFunctionCallNode).AnonymousRefBoxInstanceNode = callReturnInstance;
                    lastCallValue = Visit(call).Value;

                    var castedLastCallValue = lastCallValue as Dictionary<FunctionVariableDictionaryKey, RefBoxElement>;

                    if (castedLastCallValue != null)
                    {
                        callReturnInstance = CreateAnonymousRefBoxInstance(castedLastCallValue, anonymousRefBoxDefinitionNames);
                    }
                    else
                    {
                        break;
                    }
                }

                foreach (var name in anonymousRefBoxDefinitionNames)
                {
                    _refBoxDefinitions.Remove(name);
                }

                return ExecutionModel.FromObject(lastCallValue);
            }

            var foundFunction = instance.TryGetValue(new(refBoxFunctionCallNode.FunctionCallNode), out var element);

            if (foundFunction == false)
            {
                throw new Exception($"Invalid function name: '{refBoxFunctionCallNode.FunctionCallNode.Name}'");
            }

            var modifier = element.Modifier;

            if (modifier == AccessModifier.Guarded)
            {
                throw new Exception("Cannot call guarded function");
            }

            var runtimeState = GetRuntimeState();

            element.ScopeLevel = _scopeLevel;
            var refBoxAvailableFunctions = _functions.ToDictionary(x => x.Key, x => x.Value);
            foreach (var (functionVariable, model) in instance)
            {
                if (model.Type == RefBoxElementType.Function)
                {
                    refBoxAvailableFunctions[functionVariable.FunctionKey] = model.Function;
                }
            }

            _functions = refBoxAvailableFunctions;
            _variables = element.Variables;
            foreach (var (functionVariable, model) in instance)
            {
                if (model.Type == RefBoxElementType.Field)
                {
                    _variables.SetVariable(functionVariable.VariableName, model.Value);
                }
            }

            var callScopeLevel = element.ScopeLevel;
            element.ScopeLevel++;

            _callReturnValue = element.CallReturnValue;

            DeclareFunctionParameters(refBoxFunctionCallNode.FunctionCallNode, element.Function);

            object? returnValue = null;
            var returnEmpty = false;
            try
            {
                var body = element.Function.Body;

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
                            returnEmpty = true;
                        }

                        returnValue = callReturnValue.ReturnValue;
                        break;
                    }
                }
            }
            finally
            {
                SetRuntimeState(runtimeState);

                var refBoxFieldKeys = instance.Where(keyValue => keyValue.Value.Type == RefBoxElementType.Field).Select(x => x.Key);
                foreach (var (name, value) in element.Variables.GetVariables(0))
                {
                    if (refBoxFieldKeys.Contains(new(name)))
                    {
                        var fieldModifier = instance[new(name)].Modifier;
                        instance[new(name)] = RefBoxElement.CreateFieldElement(value, fieldModifier);
                    }
                }
            }

            if (returnEmpty)
            {
                return ExecutionModel.Empty;
            }
            return ExecutionModel.FromObject(returnValue);
        }
        else if (node is ImportNode importNode)
        {
            var pathModel = Visit(importNode.Path);
            ExceptionsHelper.FailIfFalse(pathModel.IsString(), "Expected path");
            var path = (string)pathModel;

            var content = string.Empty;
            try
            {
                content = File.ReadAllText(path);
            }
            catch (Exception)
            {
                throw new Exception($"Cannot read file {path}");
            }

            var interpreter = new Interpreter();
            try
            {
                interpreter.SetRuntimeState(GetRuntimeState());
                var lexer = new Lexer(content);
                var tokens = lexer.Tokenize();

                var parser = new Parser(tokens);
                var ast = parser.Parse();

                interpreter.Run(ast);
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception occured while executing imported script: {ex.Message}");
            }

            var runtimeState = GetRuntimeState();
            runtimeState.Combine(interpreter.GetRuntimeState());

            SetRuntimeState(runtimeState);
            return ExecutionModel.Empty;
        }
        else if (node is AnonymousValueNode anonymousValue)
        {
            return ExecutionModel.FromObject(anonymousValue.Value);
        }
        else if (node is ReadFileNode readFileNode)
        {
            var pathModel = Visit(readFileNode.Path);
            if (pathModel.IsString() == false)
            {
                throw new Exception("Expected path");
            }

            var content = File.ReadAllLines((string)pathModel);
            return ExecutionModel.FromObject(content.Cast<object>().ToList());
        }
        else if (node is ReadWholeFileNode readWholeFileNode)
        {
            var pathModel = Visit(readWholeFileNode.Path);
            if (pathModel.IsString() == false)
            {
                throw new Exception("Expected path");
            }

            var content = File.ReadAllText((string)pathModel);
            return content;
        }
        else if (node is FileExistsNode fileExistsNode)
        {
            var pathModel = Visit(fileExistsNode.Path);
            if (pathModel.IsString() == false)
            {
                throw new Exception("Expected path");
            }

            var exists = File.Exists((string)pathModel);
            return exists ? 1 : 0;
        }
        else if (node is UserDefinedExtensionNode userDefinedExtensionNode)
        {
            var typeModel = Visit(userDefinedExtensionNode.Type);
            var refboxNameModel = Visit(userDefinedExtensionNode.RefBoxName);
            var functionNameModel = Visit(userDefinedExtensionNode.FunctionName);
            var numOfParametersModel = Visit(userDefinedExtensionNode.NumberOfParameters);

            ExceptionsHelper.FailIfFalse(typeModel.IsString(), "Expected type");
            ExceptionsHelper.FailIfFalse(refboxNameModel.IsString(), "Expected an refbox name");
            ExceptionsHelper.FailIfFalse(functionNameModel.IsString(), "Expected an function name");
            ExceptionsHelper.FailIfFalse(numOfParametersModel.IsInt(), "Expected number of function parameters");

            var type = ((string)typeModel).ToLower();
            var refboxName = (string)refboxNameModel;
            var functionName = (string)functionNameModel;
            var numOfParameters = (int)numOfParametersModel;

            ExceptionsHelper.FailIfTrue(numOfParameters == 0, "Expected at least one parameter in referenced function");

            string[] types = ["string", "int", "refbox", "array"];
            ExceptionsHelper.FailIfFalse(types.Contains(type), "Invalid type");

            ExceptionsHelper.FailIfFalse(_refBoxDefinitions.ContainsKey(refboxName), "Refbox not found");
            var refbox = _refBoxDefinitions[refboxName];

            ExceptionsHelper.FailIfFalse(refbox.IsConst, "Expected const refbox");
            ExceptionsHelper.FailIfTrue(refbox.IsAbstract, "Refbox cannot be abstract");
            var function = refbox.Functions.FirstOrDefault(f => f.Element.Name == functionName && f.Element.Parameters.Count == numOfParameters);
            ExceptionsHelper.FailIfTrue(function is null, "Function not found");
            ExceptionsHelper.FailIfTrue(function.Modifier == AccessModifier.Guarded, "Function has to be Exposed");

            var functionBody = function.Element.Body;
            _userDefinedExtensions[functionName.ToLower()] = new(type, function.Element.Parameters, functionBody);

            return ExecutionModel.Empty;
        }

        throw new Exception("AST node error");
    }

    private RefBoxInstanceNode CreateAnonymousRefBoxInstance(Dictionary<FunctionVariableDictionaryKey, RefBoxElement>? callReturnValue, List<string> anonymousRefBoxDefinitionNames)
    {
        anonymousRefBoxDefinitionNames.Add($"anonymous_{Guid.NewGuid()}");
        var rawFields = callReturnValue.Where(i => i.Value.Value != null);
        var fields = rawFields.Select(rf => new RefBoxAccessModifier<AssignmentNode>(new(rf.Key.VariableName, new AnonymousValueNode(rf.Value.Value)), rf.Value.Modifier, false)).ToList();

        var rawFunc = callReturnValue.Where(i => i.Value.Function != null);
        var functions = rawFunc.Select(rf => new RefBoxAccessModifier<FunctionNode>(new(rf.Key.FunctionKey.FunctionName, rf.Value.Function.Parameters, rf.Value.Function.Body), rf.Value.Modifier, false)).ToList();

        var anonymousRefBoxNode = new RefBoxNode(anonymousRefBoxDefinitionNames.Last(), false, fields, functions, null);
        Visit(anonymousRefBoxNode); // declare anonymous RefBox
        var anonymousRefBoxInstance = new RefBoxInstanceNode(anonymousRefBoxDefinitionNames.Last());

        return anonymousRefBoxInstance;
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
        foreach (var (key, function) in _functions)
        {
            sb.AppendLine($"  {key.FunctionName} <- {{{string.Join(", ", function.Parameters)}}}");
        }

        if (_functions.Count == 0)
            sb.AppendLine("  No functions to display.");

        sb.AppendLine();

        sb.AppendLine("[RefBoxes]");
        foreach (var (name, refBox) in _refBoxDefinitions)
        {
            var fields = refBox.Fields.ConvertAll(f => $"[{f.Modifier}] {f.Element.Variable}");
            var functions = refBox.Functions.ConvertAll(f => $"[{f.Modifier}] {f.Element.Name} <- ({string.Join(", ", f.Element.Parameters)})");
            sb.AppendLine($"  {name} : {{{string.Join(", ", fields.Concat(functions))}}} {(string.IsNullOrEmpty(refBox.BaseClassName) ? "" : $"[base: {refBox.BaseClassName}]")}");
        }

        if (_refBoxDefinitions.Count == 0)
            sb.AppendLine("  No ref boxes to display.");

        Console.WriteLine(sb);
    }

    public void ResetScopesAboveRoot()
    {
        _scopeLevel = 0;
        _variables.ClearScope(1);
    }

    public InterpreterRuntimeState GetRuntimeState()
    {
        return new()
        {
            CallReturnValues = _callReturnValue,
            Functions = _functions,
            IsForLoopParameterSection = _isForLoopParameterSection,
            RefBoxDefinitions = _refBoxDefinitions,
            ScopeLevel = _scopeLevel,
            Variables = _variables
        };
    }

    public void SetRuntimeState(InterpreterRuntimeState interpreterRuntimeState)
    {
        _callReturnValue = interpreterRuntimeState.CallReturnValues;
        _functions = interpreterRuntimeState.Functions;
        _isForLoopParameterSection = interpreterRuntimeState.IsForLoopParameterSection;
        _refBoxDefinitions = interpreterRuntimeState.RefBoxDefinitions;
        _scopeLevel = interpreterRuntimeState.ScopeLevel;
        _variables = interpreterRuntimeState.Variables;
        _userDefinedExtensions = interpreterRuntimeState.UserDefinedExtensions;
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