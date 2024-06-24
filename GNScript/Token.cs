namespace GNScript;
public enum TokenType
{
    Identifier,
    Number,
    String,
    Plus,
    Minus,
    Multiply,
    Divide,
    Modulo,
    Power,
    LeftParen,
    RightParen,
    LeftBracket,
    RightBracket,
    Assign,
    Print,
    PrintInline,
    If,
    Else,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Equal,
    NotEqual,
    Function,
    Return,
    Comma,
    Semicolon,
    While,
    For,
    EndBlock,
    Input,
    Void,
    Array,
    ArrayRemoveAt,
    Colon,
    AndOperator,
    OrOperator,
    Dot,
    RefBox,
    Create,
    EOF
}

public class Token
{
    public TokenType Type { get; }
    public string Value { get; }

    public Token(TokenType type, string value)
    {
        Type = type;
        Value = value;
    }

    public override string ToString()
    {
        return $"{Type}: {Value}";
    }
}