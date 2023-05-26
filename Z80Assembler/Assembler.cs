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
    private readonly List<LabelLocation> _labelSources = new List<LabelLocation>();
    private readonly Dictionary<string, int> _variables = new Dictionary<string, int>();
    private readonly List<LabelLocation> _labelPointerRelative = new List<LabelLocation>(); // Target is single byte
    private readonly List<LabelLocation> _labelPointerAbsolute = new List<LabelLocation>(); // Target is two bytes

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

        if (_tokens.Last().GetType() == typeof(BadToken))
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
        return new BadToken(_line, _column);
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
            return new BadToken(_line, _column);
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
                    return new BadToken(_line, _column);
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
                    return new BadToken(_line, _column);
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
            return new BadToken(_line, _column);
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
            return new BadToken(_line, _column);
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

    private void AddLabelSource(string label)
    {
        _labelSources.Add(new LabelLocation(label, _assembledCode.Count - 1));
    }
    
    private void AddLabelPointerRelative(string label)
    {
        _labelPointerRelative.Add(new LabelLocation(label, _assembledCode.Count - 1));
    }
    
    private void AddLabelPointerAbsolute(string label)
    {
        _labelPointerAbsolute.Add(new LabelLocation(label, _assembledCode.Count - 1));
    }

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
                AddLabelSource(token.Label);
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

    private void WriteByte(byte value)
    {
        _assembledCode.Add(value);
    }

    private void WriteAddress(ushort address)
    {
        WriteByte((byte)address);
        WriteByte((byte)(address >> 8));
    }

    private int ResolveMath()
    {
        throw new NotImplementedException();
    }

    private void AssembleInstruction(TextToken token)
    {
        ConsumeToken();
        switch (token.Text.Length)
        {
            case 2: Assemble2CharInstruction(token);
                break;
            case 3: Assemble3CharInstruction(token);
                break;
            case 4: Assemble4CharInstruction(token);
                break;
            default:
                ConsumeTokenLine();
                _errors.Add(new SyntaxError(token));
                break;
        }
    }

    private void Assemble2CharInstruction(TextToken token)
    {
        IToken? nextToken = PeekToken();
        switch (token.Text)
        {
            case "ld": break;
            case "ex": break;
            case "jr": break;
            case "or": break;
            case "cp": break;
            case "jp": break;
            case "in": break;
            case "di": break;
            case "ei": break;
            case "rl": break;
            case "rr": break;
            case "im": break;
        }
        AssertEndOfLine(PeekToken());
    }

    private void Assemble3CharInstruction(TextToken token)
    {
        
        IToken? nextToken = PeekToken();
        switch (token.Text)
        {
            case "nop": WriteByte(0x00); break;
            case "inc":
                switch (nextToken)
                {
                    case TextToken parameterToken:
                        ConsumeToken();
                        IfIxIyInstruction(parameterToken);
                        byte? command = parameterToken.Text switch
                        {
                            "bc" => 0x03, "de" => 0x13, "hl" => 0x23, "sp" => 0x33, "ix" => 0x23, "iy" => 0x23,
                            "b" => 0x04, "c" => 0x0C, "d" => 0x14, "e" => 0x1C, "h" => 0x24, "l" => 0x2C, "a" => 0x3C,
                            _ => null
                        };
                        if (parameterToken.Text is ("ix" or "iy") && PeekToken() is TextToken halfRegToken)
                        {
                            switch (halfRegToken.Text)
                            {
                                case "h": WriteByte(0x24); return;
                                case "l": WriteByte(0x2C); return;
                            }
                        }
                        if (command == null) { TokenError(parameterToken); } else { WriteByte(command.Value); }
                        break;
                    case LBracketToken:
                        ConsumeToken();
                        IToken? registerToken = PeekToken();
                        if (registerToken is TextToken textToken)
                        {
                            IfIxIyInstruction(textToken);
                            switch (textToken.Text)
                            {
                                case "hl":
                                    ConsumeToken();
                                    if (PeekToken() is RBracketToken)
                                    {
                                        ConsumeToken();
                                        WriteByte(0x34);
                                    } else { TokenError(registerToken); }
                                    break;
                                case "ix" or "iy":
                                    ConsumeToken();
                                    WriteByte(0x34);
                                    if (PeekToken() is PlusToken)
                                    {
                                        ConsumeToken();
                                        IToken? offsetToken = PeekToken();
                                        switch (offsetToken)
                                        {
                                            case IntegerToken integerToken:
                                                int offset = integerToken.Integer;
                                                WriteByte((byte)offset);
                                                break;
                                            case VariableToken variableToken:
                                                bool variableExists = _variables.TryGetValue(variableToken.Label, out int variableOffset);
                                                WriteByte((byte)variableOffset);
                                                if (!variableExists) { TokenError(variableToken); }
                                                break;
                                            default: TokenError(offsetToken); break;
                                        }

                                        if (PeekToken() is RBracketToken) { ConsumeToken(); } else { TokenError(offsetToken); }
                                    } else { TokenError(registerToken); }
                                    break;
                                default: TokenError(textToken); break;
                            }
                        } else { TokenError(registerToken); }
                        break;
                    default:
                        TokenError(nextToken);
                        break;
                }
                break;
            case "dec":
                switch (nextToken)
                {
                    case TextToken parameterToken:
                        ConsumeToken();
                        IfIxIyInstruction(parameterToken);
                        byte? command = parameterToken.Text switch
                        {
                            "bc" => 0x0B, "de" => 0x1B, "hl" => 0x2B, "sp" => 0x3B, "ix" => 0x2B, "iy" => 0x2B,
                            "b" => 0x05, "c" => 0x0D, "d" => 0x15, "e" => 0x1D, "h" => 0x25, "l" => 0x2D, "a" => 0x3D,
                            _ => null
                        };
                        if (parameterToken.Text is ("ix" or "iy") && PeekToken() is TextToken halfRegToken)
                        {
                            switch (halfRegToken.Text)
                            {
                                case "h": WriteByte(0x25); return;
                                case "l": WriteByte(0x2D); return;
                            }
                        }
                        if (command == null) { TokenError(parameterToken); } else { WriteByte(command.Value); }
                        break;
                    case LBracketToken:
                        ConsumeToken();
                        IToken? registerToken = PeekToken();
                        if (registerToken is TextToken textToken)
                        {
                            IfIxIyInstruction(textToken);
                            switch (textToken.Text)
                            {
                                case "hl":
                                    ConsumeToken();
                                    if (PeekToken() is RBracketToken)
                                    {
                                        ConsumeToken();
                                        WriteByte(0x35);
                                    } else { TokenError(registerToken); }
                                    break;
                                case "ix" or "iy":
                                    ConsumeToken();
                                    WriteByte(0x35);
                                    if (PeekToken() is PlusToken)
                                    {
                                        ConsumeToken();
                                        IToken? offsetToken = PeekToken();
                                        switch (offsetToken)
                                        {
                                            case IntegerToken integerToken:
                                                int offset = integerToken.Integer;
                                                WriteByte((byte)offset);
                                                break;
                                            case VariableToken variableToken:
                                                bool variableExists = _variables.TryGetValue(variableToken.Label, out int variableOffset);
                                                WriteByte((byte)variableOffset);
                                                if (!variableExists) { TokenError(variableToken); }
                                                break;
                                            default: TokenError(offsetToken); break;
                                        }

                                        if (PeekToken() is RBracketToken) { ConsumeToken(); } else { TokenError(offsetToken); }
                                    } else { TokenError(registerToken); }
                                    break;
                                default: TokenError(textToken); break;
                            }
                        } else { TokenError(registerToken); }
                        break;
                    default:
                        TokenError(nextToken);
                        break;
                }
                break;
            case "add": break;
            case "rla": WriteByte(0x17); break;
            case "rra": WriteByte(0x1F); break;
            case "daa": WriteByte(0x27); break;
            case "cpl": WriteByte(0x2F); break;
            case "scf": WriteByte(0x37); break;
            case "ccf": WriteByte(0x3F); break;
            case "adc": break;
            case "sub": break;
            case "sbc": break;
            case "and": break;
            case "xor": break;
            case "ret": break;
            case "pop": break;
            case "rst": break;
            case "out": break;
            case "exx": break;
            case "rlc": break;
            case "rrc": break;
            case "sla": break;
            case "sra": break;
            case "sll": break;
            case "srl": break;
            case "bit": break;
            case "res": break;
            case "set": break;
            case "neg": break;
            case "rrd": break;
            case "rld": break;
            case "ldi": break;
            case "cpi": break;
            case "ini": break;
            case "ldd": break;
            case "cpd": break;
            case "ind": break;
        }
        AssertEndOfLine(PeekToken());
    }

    private void Assemble4CharInstruction(TextToken token)
    {
        IToken? nextToken = PeekToken();
        switch (token.Text)
        {
            case "rlca":  WriteByte(0x07); break;
            case "rrca": WriteByte(0x0F); break;
            case "djnz":
                WriteByte(0x10);
                switch (nextToken)
                {
                    case TextToken textToken:
                        ConsumeToken();
                        AddRelativeAddressLabel(textToken);
                        break;
                    case NewLineToken or null: TokenError(nextToken); break;
                    default:
                        int value = ResolveMath();
                        WriteByte((byte)(value - 2));
                        break;
                }
                break;
            case "halt": WriteByte(0x76); break;
            case "call":
                switch (nextToken)
                {
                    case TextToken parameterToken:
                        ConsumeToken();
                        if (PeekToken() is NewLineToken or null)
                        {
                            WriteByte(0xCD);
                            AddAddressLabel(parameterToken);
                        }
                        else
                        {
                            byte? command = parameterToken.Text switch
                            {
                                "nz" => 0xC4, "z" => 0xCC, "nc" => 0xD4, "c" => 0xDC,
                                "po" => 0xE4, "pe" => 0xEC, "p" => 0xF4, "m" => 0xFC,
                                _ => null
                            };
                            if (command is null) { TokenError(parameterToken); return; }
                            WriteByte(command.Value);

                            switch (PeekToken())
                            {
                                case TextToken addressToken:
                                    AddAddressLabel(addressToken);
                                    break;
                                default:
                                    int address = ResolveMath();
                                    WriteAddress((ushort)address);
                                    break;
                            }
                        }
                        break;
                    default:
                        WriteByte(0xCD);
                        int value = ResolveMath();
                        WriteAddress((ushort)value);
                        break;
                }
                break;
            case "push":
                switch (nextToken)
                {
                    case TextToken textToken:
                        ConsumeToken();
                        IfIxIyInstruction(textToken);
                        byte? command = textToken.Text switch
                        {
                            "bc" => 0xC5, "de" => 0xD5, "hl" => 0xE5, "af" => 0xF5, "ix" => 0xE5, "iy" => 0xE5,
                            _ => null
                        };
                        if (command is null) { TokenError(textToken); return; }
                        WriteByte(command.Value);
                        break;
                    default:
                        TokenError(nextToken);
                        break;
                }
                break;
            case "outi": WriteByte(0xED); WriteByte(0xA3); break;
            case "outd": WriteByte(0xED); WriteByte(0xAB); break;
            case "retn": WriteByte(0xED); WriteByte(0x45); break;
            case "reti": WriteByte(0xED); WriteByte(0x4D); break;
            case "ldir": WriteByte(0xED); WriteByte(0xB0); break;
            case "cpir": WriteByte(0xED); WriteByte(0xB1); break;
            case "inir": WriteByte(0xED); WriteByte(0xB2); break;
            case "otir": WriteByte(0xED); WriteByte(0xB3); break;
            case "lddr": WriteByte(0xED); WriteByte(0xB8); break;
            case "cpdr": WriteByte(0xED); WriteByte(0xB9); break;
            case "indr": WriteByte(0xED); WriteByte(0xBA); break;
            case "otdr": WriteByte(0xED); WriteByte(0xBB); break;
        }
        AssertEndOfLine(PeekToken());
    }

    private void TokenError(IToken? token)
    {
        _errors.Add(new SyntaxError(token ?? new BadToken(_line, _column)));
        ConsumeTokenLine();
    }

    private void IfIxIyInstruction(TextToken token)
    {
        if (token.Text is not ['i', _]) return;
        switch (token.Text[1])
        {
            case 'x':
                WriteByte(0xDD);
                break;
            case 'y':
                WriteByte(0xFD);
                break;
        }
    }

    private void AssertEndOfLine(IToken? token)
    {
        if (token is not (NewLineToken or null)) return;
        _errors.Add(new SyntaxError(token ?? new BadToken(_line, _column)));
        ConsumeTokenLine();
    }

    private void AddRelativeAddressLabel(TextToken addressToken)
    {
        ConsumeToken();
        AddLabelPointerRelative(addressToken.Text);
        WriteByte(0);
    }

    private void AddAddressLabel(TextToken addressToken)
    {
        AddLabelPointerAbsolute(addressToken.Text);
        WriteAddress(0);
    }

    #endregion

    public void InsertLabels()
    {
        
    }

    #region Helpers
    
    private static bool IsAlphanumeric(char value)
    {
        return value is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '.' or '_';
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