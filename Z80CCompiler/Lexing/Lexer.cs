using System.Globalization;
using System.Text;
using Z80CCompiler.Parsing.Tokens;

namespace Z80CCompiler.Parsing;

public class Lexer
{
    private const char EndOfFile = (char)5; 
    
    public List<IToken> Tokens { get; } = new();
    private string _code;
    private int _cursor;
    
    private StringBuilder _sb = new();
    
    public Lexer(string code)
    {
        _code = code;
    }

    public List<IToken> Lex()
    {
        while (true)
        {
            switch (Peek())
            {
                case '{': Consume(); Tokens.Add(new LBrace()); break;
                case '}': Consume(); Tokens.Add(new RBrace()); break;
                case '(': Consume(); Tokens.Add(new LBracket()); break;
                case ')': Consume(); Tokens.Add(new RBracket()); break;
                case ';': Consume(); Tokens.Add(new SemicolonToken()); break;
                case '-': Consume(); Tokens.Add(new NegationToken()); break;
                case '+': Consume(); Tokens.Add(new AdditionToken()); break;
                case '*': Consume(); Tokens.Add(new MultiplicationToken()); break;
                case '/': Consume(); Tokens.Add(new DivisionToken()); break;
                case '~': Consume(); Tokens.Add(new BitwiseComplementToken()); break;
                case '!': Consume(); Tokens.Add(new LogicalNegationToken()); break;
                case >= 'a' and <= 'z' or >= 'A' and <= 'Z': ConsumeKeywordOrIdentifier(); break;
                case >= '0' and <= '9': ConsumeIntegerLiteral(); break;
                case ' ' or '\n' or '\r': Consume(); break;
                case EndOfFile: return Tokens;
                case var character: throw new Exception(character.ToString());
            }
        }
    }

    private char Peek()
    {
        return _cursor < _code.Length ? _code[_cursor] : EndOfFile;
    }

    private void Consume()
    {
        ++_cursor;
    }

    private void Spit()
    {
        --_cursor;
    }

    private void ConsumeKeywordOrIdentifier()
    {
        _sb.Append(Peek());
        Consume();

        while (Peek() is var next and (>= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '_'))
        {
            _sb.Append(next);
            Consume();
        }

        string keywordOrIdentifier = _sb.ToString();
        switch (keywordOrIdentifier)
        {
            case "int": Tokens.Add(new IntKeywordToken()); break;
            case "return": Tokens.Add(new ReturnKeywordToken()); break;
            default: Tokens.Add(new IdentifierToken(keywordOrIdentifier)); break;
        }

        _sb.Clear();
    }

    private void ConsumeIntegerLiteral()
    {
        int value;
        if (Peek() is '0')
        {
            Consume();
            switch (Peek())
            {
                case 'x': Consume(); // Hex
                    while (Peek() is var next and (>= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F'))
                    {
                        _sb.Append(next);
                        Consume();
                    }
                    value = int.Parse(_sb.ToString(), NumberStyles.HexNumber);
                    Tokens.Add(new IntegerLiteralToken(value));
                    return;
                case 'b': Consume(); // Binary
                    while (Peek() is var next and ('0' or '1' ))
                    {
                        _sb.Append(next);
                        Consume();
                    }
                    value = Convert.ToInt32(_sb.ToString(), 2);
                    Tokens.Add(new IntegerLiteralToken(value));
                    return;
                default: Spit(); break;
            }
        }
        while (Peek() is var next and >= '0' and <= '9')
        {
            _sb.Append(next);
            Consume();
        }
        value = int.Parse(_sb.ToString());
        Tokens.Add(new IntegerLiteralToken(value));
    }
}