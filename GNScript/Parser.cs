using System.Xml.Linq;

namespace GNScript;
public class Parser
{
    private readonly List<Token> _tokens;
    private int _position;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public AstNode Parse()
    {
        AstNode startNode = null;
        AstNode node = null;
        while (_position < _tokens.Count)
        {
            if (_tokens[_position].Type == TokenType.EOF)
            {
                break;
            }

            var result = ParseStatement();

            if (startNode == null)
            {
                node = result;
                startNode = node;
                continue;
            }

            node.Next = result;
            node = node.Next;
        }
        return startNode;
    }

    private AstNode ParseStatement()
    {
        if (_tokens[_position].Type == TokenType.Function)
        {
            return ParseFunctionDefinition();
        }
        else if (_tokens[_position].Type == TokenType.Return)
        {
            _position++;
            var isVoid = TryConsumeVoid();
            if (isVoid)
                return new ReturnNode(isVoid);

            var expression = ParseExpression();
            return new ReturnNode(expression);
        }
        else if (_tokens[_position].Type == TokenType.Print)
        {
            _position++;
            var expression = ParseExpression();
            return new PrintNode(expression);
        }
        else if (_tokens[_position].Type == TokenType.PrintInline)
        {
            _position++;
            var expression = ParseExpression();
            return new PrintInlineNode(expression);
        }
        else if (_tokens[_position].Type == TokenType.Throw)
        {
            _position++;
            var expression = ParseExpression();
            return new ThrowNode(expression);
        }
        else if (_tokens[_position].Type == TokenType.Identifier)
        {
            if (_tokens[_position + 1].Type == TokenType.Assign)
            {
                var variableName = _tokens[_position].Value;
                _position += 2; // Skip identifier and assign token

                if (_tokens[_position].Type == TokenType.Input)
                {
                    var value = ParseInput();
                    return new AssignmentNode(variableName, value);
                }
                else if (_tokens[_position].Type == TokenType.Create)
                {
                    return ParseRefBoxInstance(instanceName: variableName);
                }

                var expression = ParseExpression();
                return new AssignmentNode(variableName, expression);
            }
            else if (_tokens[_position + 1].Type == TokenType.LeftParen)
            {
                return ParseFunctionCall();
            }
            if (_tokens[_position + 1].Type == TokenType.Dot)
            {
                if (_position + 3 < _tokens.Count && _tokens[_position + 3].Type == TokenType.LeftParen)
                {
                    return ParseRefBoxFunctionCall();
                }
                else
                {
                    return ParseRefBoxFieldAssigment();
                }
            }
        }
        else if (_tokens[_position].Type == TokenType.If)
        {
            _position++;
            var condition = ParseExpression();
            var thenBranch = ParseIfBody();
            AstNode elseBranch = null;

            if (_tokens[_position].Type == TokenType.Else)
            {
                _position++;
                elseBranch = ParseIfBody();
            }

            return new IfNode(condition, thenBranch, elseBranch);
        }
        else if (_tokens[_position].Type == TokenType.While)
        {
            _position++;
            var condition = ParseExpression();
            var body = ParseBody();
            return new WhileNode(condition, body);
        }
        else if (_tokens[_position].Type == TokenType.For)
        {
            _position++;
            var init = ParseStatement();
            ConsumeSemicolon();
            var condition = ParseExpression();
            ConsumeSemicolon();
            var increment = ParseStatement();
            var body = ParseBody();
            return new ForNode(init, condition, increment, body);
        }
        else if (_tokens[_position].Type == TokenType.RefBox)
        {
            return ParseRefBoxDeclaration();
        }

        return ParseExpression();
    }

    private AstNode ParseBody()
    {
        AstNode startNode = null;
        AstNode node = null;
        while (_tokens[_position].Type != TokenType.EndBlock)
        {
            var nextNode = ParseStatement();

            if (startNode == null)
            {
                startNode = nextNode;
                node = nextNode;
                continue;
            }

            node.Next = nextNode;
            node = node.Next;
        }

        _position++;

        return startNode;
    }

    private AstNode ParseIfBody()
    {
        AstNode startNode = null;
        AstNode node = null;
        while (_tokens[_position].Type != TokenType.EndBlock && _tokens[_position].Type != TokenType.Else)
        {
            var nextNode = ParseStatement();

            if (startNode == null)
            {
                startNode = nextNode;
                node = nextNode;
                continue;
            }

            node.Next = nextNode;
            node = node.Next;
        }

        if (_tokens[_position].Type == TokenType.EndBlock)
            _position++;

        return startNode;
    }

    private AstNode ParseFunctionBody()
    {
        AstNode startNode = null;
        AstNode node = null;
        while (_tokens[_position].Type != TokenType.Return)
        {
            var nextNode = ParseStatement();

            if (startNode == null)
            {
                startNode = nextNode;
                node = nextNode;
                continue;
            }

            node.Next = nextNode;
            node = node.Next;
        }

        var returnNode = ParseStatement();
        if (startNode == null)
        {
            startNode = returnNode;
            node = returnNode;
        }
        node.Next = returnNode;

        return startNode;
    }

    private void ConsumeSemicolon()
    {
        if (_tokens[_position].Type == TokenType.Semicolon)
        {
            _position++;
            return;
        }

        throw new Exception("Syntax error");
    }

    private bool TryConsumeVoid()
    {
        if (_tokens[_position].Type == TokenType.Void)
        {
            _position++;
            return true;
        }

        return false;
    }

    private AstNode ParseFunctionDefinition()
    {
        _position++; // Skip 'function' keyword
        var functionName = _tokens[_position].Value;
        _position++; // Skip function name

        List<string> parameters = new List<string>();
        _position++; // Skip left parenthesis

        while (_tokens[_position].Type != TokenType.RightParen)
        {
            parameters.Add(_tokens[_position].Value);
            _position++; // Skip parameter name

            if (_tokens[_position].Type == TokenType.Comma)
            {
                _position++; // Skip comma
            }
        }

        _position++; // Skip right parenthesis
        var body = ParseFunctionBody();

        return new FunctionNode(functionName, parameters, body);
    }

    private AstNode ParseFunctionCall()
    {
        var functionName = _tokens[_position].Value;
        _position += 2; // Skip function name and left parenthesis

        List<AstNode> arguments = new List<AstNode>();

        while (_tokens[_position].Type != TokenType.RightParen)
        {
            arguments.Add(ParseExpression());

            if (_tokens[_position].Type == TokenType.Comma)
            {
                _position++; // Skip comma
            }
        }

        // Ensure we are at a right parenthesis after parsing arguments
        if (_tokens[_position].Type != TokenType.RightParen)
        {
            throw new Exception("Missing closing parenthesis after function call arguments");
        }

        _position++; // Skip right parenthesis
        var functionCallNode = new FunctionCallNode(functionName, arguments);

        if (_tokens[_position].Type == TokenType.Colon) // there can be property after function call
        {
            return ParsePropertyAccess(functionCallNode);
        }

        return functionCallNode;
    }

    private AstNode ParseExpression()
    {
        return ParseComparison();
    }

    private AstNode ParseComparison()
    {
        var comparsionOperators = new[] { TokenType.GreaterThan, TokenType.LessThan, TokenType.Equal, TokenType.NotEqual, TokenType.GreaterThanOrEqual, TokenType.LessThanOrEqual, TokenType.AndOperator, TokenType.OrOperator };
        var left = ParseTerm();

        while (_position < _tokens.Count && comparsionOperators.Contains(_tokens[_position].Type))
        {
            var operatorToken = _tokens[_position];
            _position++;
            var right = ParseTerm();
            left = new BinaryOperationNode(left, operatorToken, right);
        }

        return left;
    }

    private AstNode ParseTerm()
    {
        var left = ParseFactor();

        while (_position < _tokens.Count &&
               (_tokens[_position].Type == TokenType.Plus || _tokens[_position].Type == TokenType.Minus))
        {
            var operatorToken = _tokens[_position];
            _position++;
            var right = ParseFactor();
            left = new BinaryOperationNode(left, operatorToken, right);
        }

        return left;
    }

    private AstNode ParseFactor()
    {
        var left = ParsePrimary();

        while (_position < _tokens.Count &&
               (_tokens[_position].Type == TokenType.Multiply || _tokens[_position].Type == TokenType.Divide || _tokens[_position].Type == TokenType.Modulo || _tokens[_position].Type == TokenType.Power))
        {
            var operatorToken = _tokens[_position];
            _position++;
            var right = ParsePrimary();
            left = new BinaryOperationNode(left, operatorToken, right);
        }

        return left;
    }

    private AstNode ParsePrimary()
    {
        var token = _tokens[_position];

        switch (token.Type)
        {
            case TokenType.Number:
                _position++;
                var numberNode = new NumberNode(token.Value);

                if (_tokens[_position].Type == TokenType.Colon)
                {
                    return ParsePropertyAccess(numberNode);
                }

                return numberNode;
            case TokenType.String:
                _position++;
                var stringNode = new StringNode(token.Value);

                if (_tokens[_position].Type == TokenType.Colon)
                {
                    return ParsePropertyAccess(stringNode);
                }

                return stringNode;
            case TokenType.Identifier:
                if (_tokens[_position + 1].Type == TokenType.LeftParen)
                {
                    return ParseFunctionCall();
                }

                if (_tokens[_position + 1].Type == TokenType.LeftBracket)
                {
                    _position++;
                    return ParseArrayAccess(new VariableNode(token.Value));
                }
                else if (_tokens[_position + 1].Type == TokenType.Colon)
                {
                    _position++;
                    return ParsePropertyAccess(new VariableNode(token.Value));
                }
                else if (_tokens[_position + 1].Type == TokenType.Dot)
                {
                    AstNode node;

                    if (_position + 3 < _tokens.Count && _tokens[_position + 3].Type == TokenType.LeftParen)
                    {
                        var functionCall = ParseRefBoxFunctionCall();
                        node = functionCall;
                    }
                    else
                    {
                        var refBoxFieldAccess = ParseRefBoxFieldAccess();
                        node = refBoxFieldAccess;
                    }

                    if (_tokens[_position].Type == TokenType.Colon) // there can be property after RefBox field/function access
                    {
                        return ParsePropertyAccess(node);
                    }

                    return node;
                }
                else
                {
                    _position++;
                    return new VariableNode(token.Value);
                }
            case TokenType.LeftParen:
                _position++;
                var expression = ParseExpression();
                if (_tokens[_position].Type != TokenType.RightParen)
                {
                    throw new Exception("Missing closing parenthesis");
                }
                _position++;
                return expression;
            case TokenType.LeftBracket:
                var array = ParseArrayDeclaration();

                if (_tokens[_position].Type == TokenType.LeftBracket)
                {
                    return ParseArrayAccess(array);
                }
                else if (_tokens[_position].Type == TokenType.Colon)
                {
                    return ParsePropertyAccess(array);
                }

                return array;
            default:
                throw new Exception($"Unexpected token: {token.Type}");
        }
    }

    private AstNode ParseRefBoxFieldAssigment()
    {
        var instanceName = _tokens[_position].Value;
        _position++; // identifier

        if (_tokens[_position].Type != TokenType.Dot)
        {
            throw new Exception("Expected dot");
        }
        _position++; // .

        if (_tokens[_position].Type != TokenType.Identifier)
        {
            throw new Exception("Expected field name");
        }
        var fieldName = _tokens[_position].Value;
        _position++; // fieldname

        if (_tokens[_position].Type != TokenType.Assign)
        {
            throw new Exception("Expected assign sign");
        }
        _position++; // =

        var value = ParseExpression();

        return new RefBoxFieldAssignmentNode(instanceName, fieldName, value);
    }

    private AstNode ParseRefBoxFieldAccess()
    {
        var instanceName = _tokens[_position].Value;
        _position += 2; // identifier and .

        if (_tokens[_position].Type != TokenType.Identifier)
        {
            throw new Exception("Expected field name");
        }

        var fieldName = _tokens[_position].Value;
        _position++; // field name

        return new RefBoxFieldAccessNode(instanceName, fieldName);
    }

    private AstNode ParseRefBoxFunctionCall()
    {
        var instanceName = _tokens[_position].Value;
        _position += 2; // identifier and .

        if (_tokens[_position].Type != TokenType.Identifier)
        {
            throw new Exception("Expected field name");
        }

        var functioncall = ParseFunctionCall() as FunctionCallNode;

        return new RefBoxFunctionCallNode(instanceName, functioncall);
    }

    private AstNode ParseInput()
    {
        var token = _tokens[_position];

        if (token.Type != TokenType.Input)
        {
            throw new Exception("Expected input");
        }
        _position++;

        return new InputNode();
    }

    private AstNode ParseArrayDeclaration()
    {
        var elements = new List<AstNode>();

        if (_tokens[_position].Type != TokenType.LeftBracket)
        {
            throw new Exception("Expected left bracket");
        }

        _position++; // [

        while (_tokens[_position].Type != TokenType.RightBracket)
        {
            elements.Add(ParseExpression());
            if (_tokens[_position].Type == TokenType.Comma)
            {
                _position++; // ,
            }
        }

        if (_tokens[_position].Type != TokenType.RightBracket)
        {
            throw new Exception("Expected right bracket");
        }

        _position++; // ]

        return new ArrayNode(elements);
    }

    private AstNode ParseArrayAccess(AstNode array)
    {
        if (_tokens[_position].Type != TokenType.LeftBracket)
        {
            throw new Exception("Expected left bracket");
        }

        _position++; // [

        var index = ParseExpression();

        if (_tokens[_position].Type != TokenType.RightBracket)
        {
            throw new Exception("Expected right bracket");
        }

        _position++; // ]

        var arrayAccess = new ArrayAccessNode(array, index);
        while (_tokens[_position].Type == TokenType.LeftBracket)
        {
            arrayAccess = ParseArrayAccess(arrayAccess) as ArrayAccessNode;
        }

        return arrayAccess;
    }

    private AstNode ParsePropertyAccess(AstNode node)
    {
        if (_tokens[_position].Type != TokenType.Colon)
        {
            throw new Exception("Expected left bracket");
        }

        _position++; // |

        var property = _tokens[_position].Value;

        _position++; // prop name

        var arguments = TryParsePropertyArguments();

        var propertyNode = new PropertyAccessNode(node, property, arguments);
        while (_tokens[_position].Type == TokenType.Colon)
        {
            propertyNode = ParsePropertyAccess(propertyNode) as PropertyAccessNode;
        }

        return propertyNode;
    }

    private AstNode ParseRefBoxDeclaration()
    {
        if (_tokens[_position].Type != TokenType.RefBox)
        {
            throw new Exception("Expected ref box declaration");
        }

        _position++; // RefBox

        var isAbstract = false;
        if (_tokens[_position].Type == TokenType.Abstract)
        {
            isAbstract = true;
            _position++; // abstract
        }

        if (_tokens[_position].Type != TokenType.Identifier)
        {
            throw new Exception("Expected ref box name");
        }

        var refBoxName = _tokens[_position].Value;
        _position++; // RefBox name

        var functions = new List<RefBoxAccessModifier<FunctionNode>>();
        var fields = new List<RefBoxAccessModifier<AssignmentNode>>();
        while (_tokens[_position].Type != TokenType.EndBlock)
        {
            var accessModifier = Enum.GetName(AccessModifier.Public);

            if (_tokens[_position].Type == TokenType.Private || _tokens[_position].Type == TokenType.Public)
            {
                accessModifier = Enum.GetName(_tokens[_position].Type);
                _position++;
            }

            Enum.TryParse<AccessModifier>(accessModifier, true, out var modifier); // public is default

            if (_tokens[_position].Type == TokenType.Function)
            {
                var function = ParseFunctionDefinition() as FunctionNode;
                functions.Add(new(function, modifier));
                continue;
            }
            // else parse field

            if (_tokens[_position].Type != TokenType.Identifier)
            {
                throw new Exception("Expected field name");
            }

            var fieldName = _tokens[_position].Value;

            _position++; // field name

            if (_tokens[_position].Type != TokenType.Assign)
            {
                throw new Exception("Expected assign");
            }

            _position++; // =

            var initialValue = ParseExpression();

            fields.Add(new(new AssignmentNode(fieldName, initialValue), modifier));
        }

        _position++; // end

        return new RefBoxNode(refBoxName, isAbstract, fields, functions);
    }

    private AstNode ParseRefBoxInstance(string instanceName)
    {
        if (_tokens[_position].Type != TokenType.Create)
        {
            throw new Exception("Expected create keyword");
        }
        _position++; // create

        if (_tokens[_position].Type != TokenType.Identifier)
        {
            throw new Exception("Expected ref box name");
        }

        var refBoxName = _tokens[_position].Value;
        _position++; // RefBox name

        return new RefBoxInstanceNode(refBoxName, instanceName);
    }

    private List<AstNode> TryParsePropertyArguments()
    {
        List<AstNode> arguments = [];
        if (_tokens[_position].Type != TokenType.LeftParen)
        {
            return arguments;
        }
        _position++; // (

        while (_tokens[_position].Type != TokenType.RightParen)
        {
            arguments.Add(ParseExpression());

            if (_tokens[_position].Type == TokenType.Comma)
            {
                _position++; // Skip comma
            }
        }

        // Ensure we are at a right parenthesis after parsing arguments
        if (_tokens[_position].Type != TokenType.RightParen)
        {
            throw new Exception("Missing closing parenthesis after function call arguments");
        }

        _position++;

        return arguments;
    }
}