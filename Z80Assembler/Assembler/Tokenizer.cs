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
            // newline 
            if (token is not null && (token is not NewLineToken || (token is NewLineToken && (_tokens.Count is 0 || _tokens.Last() is not NewLineToken))))
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

    private char PeekFurther()
    {
        return _cursor + 1 >= _code.Length ? EndOfFile : _code[_cursor + 1];
    }

    private void Consume()
    {
        if (Peek() is NewLine)
        {
            ++Line;
        }
        ++_cursor;
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
            ',' => ConsumeAndReturn(new CommaToken(Line)),
            '(' => ConsumeAndReturn(new LBracketToken(Line)),
            ')' => ConsumeAndReturn(new RBracketToken(Line)),
            '+' => ConsumeAndReturn(new PlusToken(Line)),
            '-' => ConsumeAndReturn(new MinusToken(Line)),
            '\n' => ConsumeAndReturn(new NewLineToken(Line)),
            ';' => ConsumeComment(),
            '@' => ReadConstToken(),
            '\0' => new NewLineToken(Line),
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
        _errors.Add(new SyntaxError(Line, next.ToString()));
        return new BadToken(Line);
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
            
            _errors.Add(new SyntaxError(Line));
            while (IsAlphanumeric(Peek()))
            {
                Consume();
            }
            return new BadToken(Line);
        }

        if (_buffer.Count == 0)
        {
            _errors.Add(new SyntaxError(Line, "@"));
        }

        return new VariableToken(_buffer.ToString(), Line);
    }

    private IToken ReadIntegerToken()
    {
        char start = Peek();
        if (start == '0')
        {
            char next = PeekFurther();
            switch (next)
            {
                case 'b':
                {
                    Consume(); Consume();
                    while (IsBinary(Peek()))
                    {
                        if (_buffer.Add(Peek()))
                        {
                            Consume();
                            continue;
                        }
            
                        _errors.Add(new SyntaxError(Line, "0b" + _buffer));
                        while (IsBinary(Peek()))
                        {
                            Consume();
                        }
                        return new BadToken(Line);
                    }

                    if (_buffer.Count == 0)
                    {
                        _errors.Add(new SyntaxError(Line, "0b"));
                    }

                    return new IntegerToken(ParseBinary(_buffer.ToString()), Line);
                }
                case 'x':
                {
                    Consume(); Consume();
                    while (IsHex(Peek()))
                    {
                        if (_buffer.Add(Peek()))
                        {
                            Consume();
                            continue;
                        }
            
                        _errors.Add(new SyntaxError(Line, "0x" + _buffer));
                        while (IsHex(Peek()))
                        {
                            Consume();
                        }
                        return new BadToken(Line);
                    }

                    if (_buffer.Count == 0)
                    {
                        _errors.Add(new SyntaxError(Line, "0x"));
                    }
                    return new IntegerToken(int.Parse(_buffer.ToString(), NumberStyles.HexNumber), Line);
                }
            }
        }
        
        while (IsDigit(Peek()))
        {
            if (_buffer.Add(Peek()))
            {
                Consume();
                continue;
            }
            
            _errors.Add(new SyntaxError(Line));
            while (IsDigit(Peek()))
            {
                Consume();
            }
            return new BadToken(Line);
        }

        return new IntegerToken(int.Parse(_buffer.ToString()), Line);
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
            
            _errors.Add(new SyntaxError(Line));
            while (IsAlphanumeric(Peek()))
            {
                Consume();
            }
            return new BadToken(Line);
        }

        if (Peek() != ':') return new TextToken(_buffer.ToString(), Line);
        Consume();
        return new LabelToken(_buffer.ToString(), Line);

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