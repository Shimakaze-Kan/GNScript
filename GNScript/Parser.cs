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
            var expression = ParseExpression();
            return new ReturnNode(expression);
        }
        else if (_tokens[_position].Type == TokenType.Print)
        {
            _position++;
            var expression = ParseExpression();
            return new PrintNode(expression);
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

                var expression = ParseExpression();
                return new AssignmentNode(variableName, expression);
            }
            else if (_tokens[_position + 1].Type == TokenType.LeftParen)
            {
                return ParseFunctionCall();
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
        return new FunctionCallNode(functionName, arguments);
    }

    private AstNode ParseExpression()
    {
        return ParseComparison();
    }

    private AstNode ParseComparison()
    {
        var comparsionOperators = new[] { TokenType.GreaterThan, TokenType.LessThan, TokenType.Equal, TokenType.NotEqual, TokenType.GreaterThanOrEqual, TokenType.LessThanOrEqual };
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
                return new NumberNode(token.Value);
            case TokenType.String:
                _position++;
                return new StringNode(token.Value);
            case TokenType.Identifier:
                if (_tokens[_position + 1].Type == TokenType.LeftParen)
                {
                    return ParseFunctionCall();
                }
                _position++;
                return new VariableNode(token.Value);
            case TokenType.LeftParen:
                _position++;
                var expression = ParseExpression();
                if (_tokens[_position].Type != TokenType.RightParen)
                {
                    throw new Exception("Missing closing parenthesis");
                }
                _position++;
                return expression;
            default:
                throw new Exception($"Unexpected token: {token.Type}");
        }
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
}