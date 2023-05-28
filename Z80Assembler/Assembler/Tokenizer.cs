using System.Globalization;
using Z80Assembler.Error;
using Z80Assembler.Token;

namespace Z80Assembler;

public class Tokenizer
{
    private const char EndOfFile = (char)0x05;
    private const char NewLine = '\n';
    private const char Space = ' ';
    
    private readonly string _code;
    private int _cursor;
    public int Line;
    public int Column;

    private readonly List<IToken> _tokens = new();
    private readonly StringBuffer _buffer = new(16);
    private readonly List<SyntaxError> _errors;

    public Tokenizer(string code, List<SyntaxError> errors)
    {
        _code = code;
        _errors = errors;
    }

    public List<IToken> Tokenize()
    {
        while (Peek() is not EndOfFile)
        {
            IToken? token = NextToken();
            _buffer.Clear();
            if (token is not null && (token is not NewLineToken || _tokens.Count is 0 || _tokens.Last() is NewLineToken))
            {
                _tokens.Add(token);
            }
        }

        return _tokens;
    }

    private char Peek()
    {
        return _cursor >= _code.Length ? EndOfFile : _code[_cursor];
    }

    private void Consume()
    {
        if (Peek() is NewLine)
        {
            ++Line; Column = -1;
        }
        ++_cursor;
        ++Column;
    }

    private IToken? NextToken()
    {
        char next = Peek();
        while (next is Space)
        {
            Consume();
            next = Peek();
        }

        return next switch
        {
            ',' => ConsumeAndReturn(new CommaToken(Line, Column)),
            '(' => ConsumeAndReturn(new LBracketToken(Line, Column)),
            ')' => ConsumeAndReturn(new RBracketToken(Line, Column)),
            '+' => ConsumeAndReturn(new PlusToken(Line, Column)),
            '-' => ConsumeAndReturn(new MinusToken(Line, Column)),
            '\n' => ConsumeAndReturn(new NewLineToken(Line, Column)),
            ';' => ConsumeComment(),
            '@' => ReadConstToken(),
            '\0' => new NewLineToken(Line, Column),
            >= '0' and <= '9' => ReadIntegerToken(),
            >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '.' => ReadTextOrLabel(),
            _ => InvalidInitialCharToken(next)
        };
    }

    private T ConsumeAndReturn<T>(T value)
    {
        Consume();
        return value;
    }

    private IToken InvalidInitialCharToken(char next)
    {
        Consume();
        _errors.Add(new SyntaxError(Line, Column, next.ToString()));
        return new BadToken(Line, Column);
    }
    
    private IToken? ConsumeComment()
    {
        do
        {
            Consume();
        } while (Peek() is not NewLine and not EndOfFile);

        return null;
    }
    
    private IToken ReadConstToken()
    {
        Consume();
        while (IsAlphanumeric(Peek()))
        {
            if (_buffer.Add(Peek()))
            {
                Consume();
                continue;
            }
            
            _errors.Add(new SyntaxError(Line, Column));
            while (IsAlphanumeric(Peek()))
            {
                Consume();
            }
            return new BadToken(Line, Column);
        }

        if (_buffer.Count == 0)
        {
            _errors.Add(new SyntaxError(Line, Column, "@"));
        }

        return new VariableToken(_buffer.ToString(), Line, Column);
    }

    private IToken ReadIntegerToken()
    {
        char start = Peek();
        if (start == '0')
        {
            char next = Peek();
            if (next == 'b')
            {
                Consume();
                while (IsBinary(Peek()))
                {
                    if (_buffer.Add(Peek()))
                    {
                        Consume();
                        continue;
                    }
            
                    _errors.Add(new SyntaxError(Line, Column, "0b" + _buffer));
                    while (IsBinary(Peek()))
                    {
                        Consume();
                    }
                    return new BadToken(Line, Column);
                }

                if (_buffer.Count == 0)
                {
                    _errors.Add(new SyntaxError(Line, Column, "0b"));
                }

                return new IntegerToken(ParseBinary(_buffer.ToString()), Line, Column);
            }
            if (next == 'x')
            {
                Consume();
                while (IsHex(Peek()))
                {
                    if (_buffer.Add(Peek()))
                    {
                        Consume();
                        continue;
                    }
            
                    _errors.Add(new SyntaxError(Line, Column, "0x" + _buffer));
                    while (IsHex(Peek()))
                    {
                        Consume();
                    }
                    return new BadToken(Line, Column);
                }

                if (_buffer.Count == 0)
                {
                    _errors.Add(new SyntaxError(Line, Column, "0x"));
                }
                return new IntegerToken(int.Parse(_buffer.ToString(), NumberStyles.HexNumber), Line, Column);
            }
        }

        _buffer.Add(start);
        
        while (IsDigit(Peek()))
        {
            if (_buffer.Add(Peek()))
            {
                Consume();
                continue;
            }
            
            _errors.Add(new SyntaxError(Line, Column));
            while (IsDigit(Peek()))
            {
                Consume();
            }
            return new BadToken(Line, Column);
        }

        return new IntegerToken(int.Parse(_buffer.ToString()), Line, Column);
    }

    private IToken ReadTextOrLabel()
    {
        _buffer.Add(Peek());
        Consume();
        while (IsAlphanumeric(Peek()))
        {
            if (_buffer.Add(Peek()))
            {
                Consume();
                continue;
            }
            
            _errors.Add(new SyntaxError(Line, Column));
            while (IsAlphanumeric(Peek()))
            {
                Consume();
            }
            return new BadToken(Line, Column);
        }

        if (Peek() != ':') return new TextToken(_buffer.ToString(), Line, Column);
        Consume();
        return new LabelToken(_buffer.ToString(), Line, Column);

    }
    private static bool IsAlphanumeric(char value)
    {
        return value is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '.' or '_' or '\'';
    }

    private static bool IsDigit(char value)
    {
        return value is >= '0' and <= '9';
    }

    private static bool IsHex(char value)
    {
        return value is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
    }

    private static bool IsBinary(char value)
    {
        return value is '0' or '1';
    }

    private static int ParseBinary(string binary)
    {
        int value = 0;
        foreach (int bit in binary.Select(charBit => charBit switch
                 {
                     '0' => 0,
                     '1' => 1,
                     _ => throw new FormatException()
                 }))
        {
            value *= 2;
            value += bit;
        }

        return value;
    }
}