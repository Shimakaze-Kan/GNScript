using System.Linq;
using System.Text;
using GNScript.Exceptions;
using GNScript.Helpers;
using GNScript.Models;

namespace GNScript;
public class Interpreter
{
    private VariableCollection _variables = new();
    private readonly Dictionary<string, FunctionNode> _functions = [];
    private readonly Dictionary<string, RefBoxNode> _refBoxDefinitions = [];
    private Stack<CallReturnValue> _callReturnValue = [];
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
                        return ExecutionModel.FromObject(leftValueArray.Chunk(rightValueInt));
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
                        throw new Exception($"Nieznany operator: {binaryNode.Operator.Type}");
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
                        throw new Exception($"Nieznany operator: {binaryNode.Operator.Type}");
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
                        throw new Exception($"Nieznany operator: {binaryNode.Operator.Type}");
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
                        throw new Exception($"Nieznany operator: {binaryNode.Operator.Type}");
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
            Console.Write(model.ToPrintable());
            return ExecutionModel.Empty;
        }
        else if (node is PrintNode printNode)
        {
            var model = Visit(printNode.Expression);
            Console.WriteLine(model.ToPrintable());
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
                else if (EnumHelpers.EqualsIgnoreCase(propertyNode.PropertyName, ArrayProperty.Has))
                {
                    ExceptionsHelper.FailIfTrue(propertyNode.Arguments.Count != 1, "Expected 1 arguments");
                    var valueModel = Visit(propertyNode.Arguments[0]);

                    return arrayValue.Contains(valueModel.Value) ? 1 : 0;
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
                else if (EnumHelpers.EqualsIgnoreCase(propertyNode.PropertyName, StringProperty.Length))
                {
                    return ExecutionModel.FromObject(stringValue.Length);
                }
            }

            var refBoxProperties = EnumHelpers.GetEnumNamesLowercase<BoxProperty>();
            if (nodeModel.IsRefBox() && refBoxProperties.Contains(propertyNode.PropertyName))
            {
                if (EnumHelpers.EqualsIgnoreCase(propertyNode.PropertyName, BoxProperty.IsInstanceOf))
                {
                    ExceptionsHelper.FailIfTrue(propertyNode.Arguments.Count != 1, "Expected 1 arguments");
                    var valueModel = Visit(propertyNode.Arguments[0]);
                    ExceptionsHelper.FailIfFalse(valueModel.IsString(), "Expected ref box name");
                    var refBoxName = (string)valueModel;

                    var instanceName = ((VariableNode)propertyNode.Node).Name;
                    var refBox = (Dictionary<string, RefBoxElement>)_variables.GetVariable(instanceName, _scopeLevel);
                    var instanceFields = refBox.Keys.ToList();
                    var definitionFields = _refBoxDefinitions[refBoxName].Fields.ConvertAll(field => field.Element.Variable);

                    var sameFields = Enumerable.SequenceEqual(definitionFields.Order(), instanceFields.Order());
                    return sameFields ? 1 : 0;
                }
                else if (EnumHelpers.EqualsIgnoreCase(propertyNode.PropertyName, BoxProperty.HasField))
                {
                    ExceptionsHelper.FailIfTrue(propertyNode.Arguments.Count != 1, "Expected 1 arguments");
                    var valueModel = Visit(propertyNode.Arguments[0]);
                    ExceptionsHelper.FailIfFalse(valueModel.IsString(), "Expected field name");
                    var fieldName = (string)valueModel;

                    var instanceName = ((VariableNode)propertyNode.Node).Name;
                    var refBox = (Dictionary<string, RefBoxElement>)_variables.GetVariable(instanceName, _scopeLevel);
                    var instanceFields = refBox.Keys.ToList();

                    return instanceFields.Contains(fieldName) ? 1 : 0;
                }
            }

            throw new Exception($"Property '{propertyNode.PropertyName}' not found");
        }
        else if (node is RefBoxNode refBoxNode)
        {
            var fieldNames = refBoxNode.Fields.ConvertAll(f => f.Element.Variable);
            ExceptionsHelper.FailIfFalse(fieldNames.Count == fieldNames.Distinct().Count(), "Field name duplication");

            var funcNames = refBoxNode.Functions.ConvertAll(f => f.Element.Name);
            ExceptionsHelper.FailIfFalse(funcNames.Count == funcNames.Distinct().Count(), "Func name duplication");

            var fieldFuncNamesIntersect = fieldNames.Intersect(funcNames);
            ExceptionsHelper.FailIfFalse(fieldFuncNamesIntersect.Count() == 0, "Function and field cannot have the same name");

            if (string.IsNullOrEmpty(refBoxNode.BaseClassName) == false)
            {
                ExceptionsHelper.FailIfFalse(_refBoxDefinitions.TryGetValue(refBoxNode.BaseClassName, out var baseRefBoxDefinition),
                    $"Base ref box '{refBoxNode.BaseClassName}' not found");

                foreach (var field in baseRefBoxDefinition.Fields)
                {
                    if (field.Modifier == AccessModifier.Private)
                        continue;
                    if (refBoxNode.Fields.Any(f => f.Element.Variable == field.Element.Variable))
                        continue;

                    refBoxNode.Fields.Add(field);
                }

                foreach (var func in baseRefBoxDefinition.Functions)
                {
                    if (func.Modifier == AccessModifier.Private)
                        continue;
                    if (refBoxNode.Functions.Any(f => f.Element.Name == func.Element.Name))
                        continue;

                    refBoxNode.Functions.Add(func);
                }
            }

            _refBoxDefinitions[refBoxNode.Name] = refBoxNode;
            return ExecutionModel.Empty;
        }
        else if (node is RefBoxInstanceNode refBoxInstanceNode)
        {
            var refBoxDefinition = _refBoxDefinitions[refBoxInstanceNode.RefBoxName];
            var instance = new Dictionary<string, RefBoxElement>();
            ExceptionsHelper.FailIfTrue(refBoxDefinition.IsAbstract, "Cannot make instance of abstract ref box");

            foreach (var field in refBoxDefinition.Fields)
            {
                instance[field.Element.Variable] = RefBoxElement.CreateFieldElement(Visit(field.Element.Expression).Value, field.Modifier);
            }

            foreach (var func in refBoxDefinition.Functions)
            {
                instance[func.Element.Name] = RefBoxElement.CreateFunctionElement(func.Element, func.Modifier);
            }

            _variables.SetVariable(refBoxInstanceNode.InstanceName, instance);

            return ExecutionModel.Empty;
        }
        else if (node is RefBoxFieldAccessNode refBoxFieldAccessNode)
        {
            var instance = (Dictionary<string, RefBoxElement>)_variables.GetVariable(refBoxFieldAccessNode.InstanceName, _scopeLevel);

            if (instance[refBoxFieldAccessNode.FieldName].Modifier == AccessModifier.Private)
            {
                throw new Exception("Cannot access private field");
            }

            return ExecutionModel.FromObject(instance[refBoxFieldAccessNode.FieldName].Value);
        }
        else if (node is RefBoxFieldAssignmentNode refBoxFieldAssignmentNode)
        {
            var instance = (Dictionary<string, RefBoxElement>)_variables.GetVariable(refBoxFieldAssignmentNode.InstanceName, _scopeLevel);
            var modifier = instance[refBoxFieldAssignmentNode.FieldName].Modifier;

            if (modifier == AccessModifier.Private)
            {
                throw new Exception("Cannot set value to private field");
            }

            instance[refBoxFieldAssignmentNode.FieldName] = RefBoxElement.CreateFieldElement(Visit(refBoxFieldAssignmentNode.Value).Value, modifier);
            return ExecutionModel.Empty;
        }
        else if (node is ThrowNode throwNode)
        {
            var message = (string)Visit(throwNode.Message);
            throw new UserDefinedException(message);
        }
        else if (node is RefBoxFunctionCallNode refBoxFunctionCallNode)
        {
            var instance = (Dictionary<string, RefBoxElement>)_variables.GetVariable(refBoxFunctionCallNode.InstanceName, _scopeLevel);
            var foundFunction = instance.TryGetValue(refBoxFunctionCallNode.FunctionCallNode.Name, out var element);

            if (foundFunction == false)
            {
                throw new Exception($"Invalid function name: '{refBoxFunctionCallNode.FunctionCallNode.Name}'");
            }

            var modifier = element.Modifier;

            if (modifier == AccessModifier.Private)
            {
                throw new Exception("Cannot call private function");
            }

            var globalScopeLevel = _scopeLevel;
            element.ScopeLevel = globalScopeLevel;

            var globalVariables = _variables;
            _variables = element.Variables;

            foreach (var (name, model) in instance)
            {
                if (model.Type == RefBoxElementType.Field)
                {
                    _variables.SetVariable(name, model.Value);
                }
            }

            var callScopeLevel = element.ScopeLevel;
            element.ScopeLevel++;

            var globalCallStack = _callReturnValue;
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
                _variables = globalVariables; // restore global variables
                _callReturnValue = globalCallStack; // restore global call stack
                _scopeLevel = globalScopeLevel; // restore scope level

                var refBoxFieldKeys = instance.Where(keyValue => keyValue.Value.Type == RefBoxElementType.Field).Select(x => x.Key);
                foreach (var (name, value) in element.Variables.GetVariables(0))
                {
                    if (refBoxFieldKeys.Contains(name))
                    {
                        var fieldModifier = instance[name].Modifier;
                        instance[name] = RefBoxElement.CreateFieldElement(value, fieldModifier);
                    }
                }
            }

            if (returnEmpty)
            {
                return ExecutionModel.Empty;
            }
            return ExecutionModel.FromObject(returnValue);
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