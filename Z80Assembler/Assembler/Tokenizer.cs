using System.Globalization;
using Z80Assembler.Error;
using Z80Assembler.Token;

namespace Z80Assembler;

public class Tokenizer
{
    private const char EndOfFile = (char)0x05;
    private const char NewLine = '\n';
    private const char Space = ' ';
    
    private string _code;
    private int _cursor;
    public int Line = 1;
    
    private Dictionary<string, IToken[]> _macros = new();

    public List<IToken> Tokens { get; } = new();
    private readonly StringBuffer _buffer = new(64);
    private readonly List<SyntaxError> _errors;

    public Tokenizer(List<SyntaxError> errors)
    {
        _errors = errors;
    }

    public void Tokenize(string code)
    {
        _code = code;
        Line = 1;
        _cursor = 0;
        // TODO file name for errors
        while (Peek() is not EndOfFile)
        {
            IToken? token = NextToken();
            _buffer.Clear();
            // newline 
            if (token is not null && (token is not NewLineToken || (token is NewLineToken && (Tokens.Count is 0 || Tokens.Last() is not NewLineToken))))
            {
                Tokens.Add(token);
            }
        }
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
            '\0' => new NewLineToken(Line),
            >= '0' and <= '9' => ReadIntegerToken(),
            '\'' => ReadIntegerTokenChar(),
            '"' => ReadStringToken(),
            >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '.' or '_' => ReadTextOrLabel(),
            '%' => ConsumeDefine(),
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

    private IToken? ConsumeDefine()
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

        bool undefine = false;
        switch (_buffer.ToString())
        {
            case "undefine": undefine = true; break;
            case not "define": return new BadToken(Line);
        }
        
        _buffer.Clear();

        if (Peek() is not Space)
        {
            return new BadToken(Line);
        }
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

        string macroName = _buffer.ToString();
        if (undefine)
        {
            return _macros.Remove(macroName) ? null : new BadToken(Line);
        }

        if (Peek() is not Space)
        {
            return new BadToken(Line);
        }
        Consume();

        List<IToken> tokens = new();
        while (Peek() is not NewLine and not EndOfFile)
        {
            _buffer.Clear();
            tokens.Add(NextToken()!);
        }
        _macros.Add(macroName, tokens.ToArray());
        return null;
    }
    
    private IToken? ConsumeComment()
    {
        do
        {
            Consume();
        } while (Peek() is not NewLine and not EndOfFile);

        return null;
    }

    private IToken ReadIntegerTokenChar()
    {
        Consume();
        char value = Peek();
        Consume();
        if (Peek() == '\'')
        {
            Consume();
            return new IntegerToken(value, Line);
        }
        return new BadToken(Line);
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

    private IToken? ReadTextOrLabel()
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

        if (Peek() != ':')
        {
            string text = _buffer.ToString();

            if (!_macros.TryGetValue(text, out IToken[]? tokens)) return new TextToken(text, Line);
            
            foreach (IToken token in tokens)
            {
                Tokens.Add(token);
            }
            return null;

        }
        Consume();
        return new LabelToken(_buffer.ToString(), Line);

    }

    private IToken ReadStringToken()
    {
        Consume();
        while (Peek() is not '"' and not EndOfFile and not NewLine)
        {
            if (_buffer.Add(Peek()))
            {
                Consume();
                continue;
            }
            
            _errors.Add(new SyntaxError(Line));
            while (Peek() is not EndOfFile and not NewLine)
            {
                Consume();
            }
            return new BadToken(Line);
        }

        if (Peek() == '"')
        {
            Consume();
            return new StringToken(_buffer.ToString(), Line);
        }
        _errors.Add(new SyntaxError(Line));
        return new BadToken(Line);
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