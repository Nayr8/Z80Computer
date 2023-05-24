using System.Globalization;
using Z80Assembler.Error;
using Z80Assembler.Token;

namespace Z80Assembler;

public class Assembler
{
    private readonly string _code;
    private int _cursor = 0;
    private int _line = 0;
    private int _column = 0;
    private bool _end = false;

    private List<IToken> _tokens = new List<IToken>();
    private StringBuffer _buffer = new StringBuffer(16);

    private List<SyntaxError> _errors = new List<SyntaxError>();

    public Assembler(string code)
    {
        // Normalise line endings
        code = code.Replace("\r\n", "\n");
        code = code.Replace("\r", "\n");
        
        _code = code;
    }

    public void Tokenize()
    {
        while (!_end)
        {
            _tokens.Add(NextToken());
        }

        if (_tokens.Last().GetType() == typeof(BrokenToken))
        {
            _tokens.RemoveAt(_tokens.Count - 1);
        }
    }

    private char Next()
    {
        if (_code[_cursor] == ';')
        {
            while (_cursor < _code.Length && Peek() != '\n')
            {
                ++_cursor;
                ++_column;
            }
            if (_cursor == _code.Length)
            {
                _end = true;
            }
            NewLine();
            return '\n';
        }
        char value = _code[_cursor];
        ++_cursor;
        ++_column;
        if (_cursor == _code.Length)
        {
            _end = true;
        }
        if (value == '\n')
        {
            NewLine();
        }

        return value;
    }

    private char Peek()
    {
        return _end ? '\0' : _code[_cursor];
    }

    private void NewLine()
    {
        ++_line;
        _column = -1;
    }

    private IToken NextToken()
    {
        char value;
        do
        {
            if (_end)
            {
                return new BrokenToken();
            }
            value = Next();
        } while (value is ' ' or '\n');

        IToken token = value switch
        {
            ',' => new CommaToken(),
            '(' => new LBracketToken(),
            ')' => new RBracketToken(),
            '+' => new PlusToken(),
            '-' => new MinusToken(),
            '@' => ReadConstToken(),
            >= '0' and <= '9' => ReadIntegerToken(value),
            >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '.' => ReadTextOrLabel(value),
            _ => InvalidInitialCharToken(value)
        };
        _buffer.Clear();
        
        return token;
    }

    private IToken InvalidInitialCharToken(char start)
    {
        _errors.Add(new SyntaxError(_line, _column, start.ToString()));
        return new BrokenToken();
    }

    private IToken ReadConstToken()
    {
        while (IsAlphanumeric(Peek()))
        {
            if (_buffer.Add(Next())) continue;
            
            _errors.Add(new SyntaxError(_line, _column));
            while (IsAlphanumeric(Peek()))
            {
                Next();
            }
            return new BrokenToken();
        }

        if (_buffer.Count == 0)
        {
            _errors.Add(new SyntaxError(_line, _column, "@"));
        }

        return new ConstToken(_buffer.ToString());
    }

    private IToken ReadIntegerToken(char start)
    {
        if (start == '0')
        {
            char next = Peek();
            if (next == 'b')
            {
                Next();
                while (IsBinary(Peek()))
                {
                    if (_buffer.Add(Next())) continue;
            
                    _errors.Add(new SyntaxError(_line, _column, "0b" + _buffer));
                    while (IsBinary(Peek()))
                    {
                        Next();
                    }
                    return new BrokenToken();
                }

                if (_buffer.Count == 0)
                {
                    _errors.Add(new SyntaxError(_line, _column, "0b"));
                }

                return new IntegerToken(ParseBinary(_buffer.ToString()));
            }
            if (next == 'x')
            {
                Next();
                while (IsHex(Peek()))
                {
                    if (_buffer.Add(Next())) continue;
            
                    _errors.Add(new SyntaxError(_line, _column, "0x" + _buffer));
                    while (IsHex(Peek()))
                    {
                        Next();
                    }
                    return new BrokenToken();
                }

                if (_buffer.Count == 0)
                {
                    _errors.Add(new SyntaxError(_line, _column, "0x"));
                }
                return new IntegerToken(int.Parse(_buffer.ToString(), NumberStyles.HexNumber));
            }
        }

        _buffer.Add(start);
        
        while (IsDigit(Peek()))
        {
            if (_buffer.Add(Next())) continue;
            
            _errors.Add(new SyntaxError(_line, _column));
            while (IsDigit(Peek()))
            {
                Next();
            }
            return new BrokenToken();
        }

        return new IntegerToken(int.Parse(_buffer.ToString()));
    }

    private IToken ReadTextOrLabel(char start)
    {
        _buffer.Add(start);
        while (IsAlphanumeric(Peek()))
        {
            if (_buffer.Add(Next())) continue;
            
            _errors.Add(new SyntaxError(_line, _column));
            while (IsAlphanumeric(Peek()))
            {
                Next();
            }
            return new BrokenToken();
        }

        if (Peek() != ':') return new TextToken(_buffer.ToString());
        Next();
        return new LabelToken(_buffer.ToString());

    }

    private static bool IsAlphanumeric(char value)
    {
        return value is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '.';
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
        foreach (var bit in binary.Select(charBit => charBit switch
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
    
    public void DumpErrors()
    {
        foreach (var error in _errors)
        {
            Console.WriteLine(error);
        }
    }

    public void DumpTokens()
    {
        foreach (var token in _tokens)
        {
            Console.WriteLine(token);
        }
    }
}