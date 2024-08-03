namespace GNScript;
public class Lexer
{
    private string _input;
    private int _position;

    public Lexer(string input)
    {
        _input = input;
        _position = 0;
    }

    private Token ReadString()
    {
        int start = _position;
        _position++; // Skip initial quote

        while (_position < _input.Length)
        {
            if (_input[_position] == '"' && (_position == 0 || _input[_position - 1] != '\\'))
            {
                break;
            }
            _position++;
        }

        if (_position >= _input.Length)
        {
            throw new Exception("Unterminated string literal");
        }

        _position++; // Skip closing quote
        string value = _input.Substring(start + 1, _position - start - 2);
        return new Token(TokenType.String, value);
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while (_position < _input.Length)
        {
            if (char.IsWhiteSpace(_input[_position]))
            {
                _position++;
                continue;
            }

            char current = _input[_position];
            switch (current)
            {
                case '+':
                    tokens.Add(new Token(TokenType.Plus, "+"));
                    _position++;
                    break;
                case '-':
                    tokens.Add(new Token(TokenType.Minus, "-"));
                    _position++;
                    break;
                case '*':
                    if (_input[_position + 1] == '*')
                    {
                        tokens.Add(new Token(TokenType.Power, "**"));
                        _position += 2;
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Multiply, "*"));
                        _position++;
                    }
                    break;
                case '/':
                    tokens.Add(new Token(TokenType.Divide, "/"));
                    _position++;
                    break;
                case '%':
                    tokens.Add(new Token(TokenType.Modulo, "%"));
                    _position++;
                    break;
                case '(':
                    tokens.Add(new Token(TokenType.LeftParen, "("));
                    _position++;
                    break;
                case ')':
                    tokens.Add(new Token(TokenType.RightParen, ")"));
                    _position++;
                    break;
                case '[':
                    tokens.Add(new Token(TokenType.LeftBracket, "["));
                    _position++;
                    break;
                case ']':
                    tokens.Add(new Token(TokenType.RightBracket, "]"));
                    _position++;
                    break;
                case '~':
                    tokens.Add(new Token(TokenType.ArrayRemoveAt, "~"));
                    _position++;
                    break;
                case ':':
                    tokens.Add(new Token(TokenType.Colon, ":"));
                    _position++;
                    break;
                case '.':
                    tokens.Add(new Token(TokenType.Dot, "."));
                    _position++;
                    break;
                case '&' when _input[_position + 1] == '&':
                    tokens.Add(new Token(TokenType.AndOperator, "&&"));
                    _position += 2;
                    break;
                case '|' when _input[_position + 1] == '|':
                    tokens.Add(new Token(TokenType.OrOperator, "||"));
                    _position += 2;
                    break;
                case '=':
                    if (_input[_position + 1] == '=')
                    {
                        tokens.Add(new Token(TokenType.Equal, "=="));
                        _position += 2;
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Assign, "="));
                        _position++;
                    }
                    break;
                case '>':
                    if (_input[_position + 1] == '=')
                    {
                        tokens.Add(new Token(TokenType.GreaterThanOrEqual, ">="));
                        _position += 2;
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.GreaterThan, ">"));
                        _position++;
                    }
                    break;
                case '<':
                    if (_input[_position + 1] == '>')
                    {
                        tokens.Add(new Token(TokenType.NotEqual, "<>"));
                        _position += 2;
                    }
                    else if (_input[_position + 1] == '=')
                    {
                        tokens.Add(new Token(TokenType.LessThanOrEqual, "<="));
                        _position += 2;
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.LessThan, "<"));
                        _position++;
                    }
                    break;
                case ',':
                    tokens.Add(new Token(TokenType.Comma, ","));
                    _position++;
                    break;
                case ';':
                    tokens.Add(new Token(TokenType.Semicolon, ";"));
                    _position++;
                    break;
                case '"':
                    tokens.Add(ReadString());
                    break;
                default:
                    if (char.IsDigit(current))
                    {
                        var start = _position;
                        while (_position < _input.Length && char.IsDigit(_input[_position]))
                        {
                            _position++;
                        }
                        var value = _input.Substring(start, _position - start);
                        tokens.Add(new Token(TokenType.Number, value));
                    }
                    else if (IsValidFirstCharacterOfIdentifier(current))
                    {
                        tokens.Add(ReadIdentifierOrKeyword());
                    }
                    else
                    {
                        throw new Exception($"Unknown character: {current}");
                    }
                    break;
            }
        }

        tokens.Add(new Token(TokenType.EOF, ""));
        return tokens;
    }

    private Token ReadIdentifierOrKeyword()
    {
        int start = _position;
        while (_position < _input.Length && IsValidIdentifierCharacter(_input[_position]))
        {
            _position++;
        }
        string value = _input.Substring(start, _position - start);

        switch (value)
        {
            case "print":
                return new Token(TokenType.Print, value);
            case "printInline":
                return new Token(TokenType.PrintInline, value);
            case "if":
                return new Token(TokenType.If, value);
            case "else":
                return new Token(TokenType.Else, value);
            case "function":
                return new Token(TokenType.Function, value);
            case "return":
                return new Token(TokenType.Return, value);
            case "while":
                return new Token(TokenType.While, value);
            case "for":
                return new Token(TokenType.For, value);
            case "end":
                return new Token(TokenType.EndBlock, value);
            case "input":
                return new Token(TokenType.Input, value);
            case "refbox":
                return new Token(TokenType.RefBox, value);
            case "create":
                return new Token(TokenType.Create, value);
            case "throw":
                return new Token(TokenType.Throw, value);
            case "guarded":
                return new Token(TokenType.Guarded, value);
            case "exposed":
                return new Token(TokenType.Exposed, value);
            case "abstract":
                return new Token(TokenType.Abstract, value);
            case "import":
                return new Token(TokenType.Import, value);
            case "readFile":
                return new Token(TokenType.ReadFile, value);
            case "readWholeFile":
                return new Token(TokenType.ReadWholeFile, value);
            case "fileExists":
                return new Token(TokenType.FileExists, value);
            case "const":
                return new Token(TokenType.Const, value);
            case "wuwei":
            case "void":
                return new Token(TokenType.Void, "void");
            default:
                return new Token(TokenType.Identifier, value);
        }
    }

    private static bool IsValidIdentifierCharacter(char ch)
    {
        return char.IsLetter(ch) || char.IsAsciiDigit(ch) || ch == '_';
    }

    private static bool IsValidFirstCharacterOfIdentifier(char ch)
    {
        return char.IsLetter(ch) || ch == '_'; 
    }
}