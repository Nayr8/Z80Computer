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

    private readonly List<IToken> _tokens = new List<IToken>();
    private readonly StringBuffer _buffer = new StringBuffer(16);
    private readonly List<SyntaxError> _errors = new List<SyntaxError>();

    private int _tokenCursor = 0;

    private readonly List<byte> _assembledCode = new List<byte>();
    private readonly List<LabelLocation> _labels = new List<LabelLocation>();
    private readonly Dictionary<string, int> _variables = new Dictionary<string, int>();

    public Assembler(string code)
    {
        // Normalise line endings
        code = code.Replace("\r\n", "\n");
        code = code.Replace("\r", "\n");
        
        _code = code;
    }

    public byte[] Assemble()
    {
        Tokenize();
        AssembleToBinary();
        InsertLabels();
        return _assembledCode.ToArray();
    }

    public void Tokenize()
    {
        while (!_end)
        {
            IToken token = NextToken();
            if (token.GetType() == typeof(NewLineToken) && _tokens.Count > 0 && _tokens.Last().GetType() == typeof(NewLineToken))
            {
                continue;
            }
            _tokens.Add(token);
        }

        if (_tokens.Last().GetType() == typeof(BrokenToken))
        {
            _tokens.RemoveAt(_tokens.Count - 1);
        }
    }

    #region Tokenize
    
    private void Consume()
    {
        if (Peek() == '\n')
        {
            NewLine();
        }
        ++_cursor;
        ++_column;
        _end = _cursor >= _code.Length;
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
        char value = Peek();
        while (value is ' ')
        {
            Consume();
            value = Peek();
        }

        IToken token = value switch
        {
            ',' => ConsumeAndReturn(new CommaToken(_line, _column)),
            '(' => ConsumeAndReturn(new LBracketToken(_line, _column)),
            ')' => ConsumeAndReturn(new RBracketToken(_line, _column)),
            '+' => ConsumeAndReturn(new PlusToken(_line, _column)),
            '-' => ConsumeAndReturn(new MinusToken(_line, _column)),
            '\n' => ConsumeAndReturn(new NewLineToken(_line, _column)),
            ';' => ConsumeComment(),
            '@' => ReadConstToken(),
            '\0' => new NewLineToken(_line, _column),
            >= '0' and <= '9' => ReadIntegerToken(),
            >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '.' => ReadTextOrLabel(),
            _ => InvalidInitialCharToken()
        };
        _buffer.Clear();
        
        return token;
    }

    private T ConsumeAndReturn<T>(T value)
    {
        Consume();
        return value;
    }

    private IToken ConsumeComment()
    {
        do
        {
            Consume();
        } while (Peek() is not '\n' and not '\0');

        return NextToken();
    }

    private IToken InvalidInitialCharToken()
    {
        char value = Peek();
        Consume();
        _errors.Add(new SyntaxError(_line, _column, value.ToString()));
        return new BrokenToken(_line, _column);
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
            
            _errors.Add(new SyntaxError(_line, _column));
            while (IsAlphanumeric(Peek()))
            {
                Consume();
            }
            return new BrokenToken(_line, _column);
        }

        if (_buffer.Count == 0)
        {
            _errors.Add(new SyntaxError(_line, _column, "@"));
        }

        return new VariableToken(_buffer.ToString(), _line, _column);
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
            
                    _errors.Add(new SyntaxError(_line, _column, "0b" + _buffer));
                    while (IsBinary(Peek()))
                    {
                        Consume();
                    }
                    return new BrokenToken(_line, _column);
                }

                if (_buffer.Count == 0)
                {
                    _errors.Add(new SyntaxError(_line, _column, "0b"));
                }

                return new IntegerToken(ParseBinary(_buffer.ToString()), _line, _column);
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
            
                    _errors.Add(new SyntaxError(_line, _column, "0x" + _buffer));
                    while (IsHex(Peek()))
                    {
                        Consume();
                    }
                    return new BrokenToken(_line, _column);
                }

                if (_buffer.Count == 0)
                {
                    _errors.Add(new SyntaxError(_line, _column, "0x"));
                }
                return new IntegerToken(int.Parse(_buffer.ToString(), NumberStyles.HexNumber), _line, _column);
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
            
            _errors.Add(new SyntaxError(_line, _column));
            while (IsDigit(Peek()))
            {
                Consume();
            }
            return new BrokenToken(_line, _column);
        }

        return new IntegerToken(int.Parse(_buffer.ToString()), _line, _column);
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
            
            _errors.Add(new SyntaxError(_line, _column));
            while (IsAlphanumeric(Peek()))
            {
                Consume();
            }
            return new BrokenToken(_line, _column);
        }

        if (Peek() != ':') return new TextToken(_buffer.ToString(), _line, _column);
        Consume();
        return new LabelToken(_buffer.ToString(), _line, _column);

    }
    
    #endregion

    public void AssembleToBinary()
    {
        if (_tokens.Count == 0)
        {
            return;
        }

        while (PeekToken() is not null)
        {
            AssembleLine();
        }
    }

    #region AssembleToBinary

    private IToken? PeekToken()
    {
        return _end ? _tokens[_tokenCursor] : null;
    }

    private IToken? TakeToken()
    {
        IToken? token = PeekToken();
        ConsumeToken();
        return token;
    }

    private void ConsumeToken()
    {
        ++_tokenCursor;
        _end = _tokenCursor < _tokens.Count;
    }

    private void ConsumeTokenLine()
    {
        while (PeekToken() is not NewLineToken or null)
        {
            ConsumeToken();
        }
    }

    private void AssembleLine()
    {
        switch (TakeToken())
        {
            case LabelToken token:
                _labels.Add(new LabelLocation(token.Label, token.Line, token.Column));
                break;
            case VariableToken token:
                int value = ResolveMath();
                _variables.Add(token.Label, value);
                break;
            case TextToken token:
                AssembleInstruction(token);
                break;
            case { } token:
                ConsumeTokenLine();
                _errors.Add(new SyntaxError(token));
                break;
        }
    }

    private int ResolveMath()
    {
        throw new NotImplementedException();
    }

    private void AssembleInstruction(TextToken token)
    {
        switch (token.Text.Length)
        {
            case 2: // TODO
                break;
            case 3: // TODO
                break;
            case 4: // TODO
                break;
            default:
                ConsumeTokenLine();
                _errors.Add(new SyntaxError(token));
                break;
        }
    }

    #endregion

    public void InsertLabels()
    {
        
    }

    #region Helpers
    
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
    
    #endregion
    
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
            Console.Write(token.ToString() + ' ');
        }
    }
}