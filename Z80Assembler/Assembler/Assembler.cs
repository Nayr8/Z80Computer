using Z80Assembler.Error;
using Z80Assembler.Token;

namespace Z80Assembler;

public class Assembler
{
    private readonly List<SyntaxError> _errors = new();

    private readonly Tokenizer _tokenizer;
    private List<IToken> _tokens = new();

    private int _cursor;

    private readonly List<byte> _assembledCode = new();
    private readonly Dictionary<string, int> _labelSources= new();
    private readonly Dictionary<string, int> _variables = new();
    private readonly List<LabelLocation> _labelPointerRelative = new(); // Target is single byte
    private readonly List<LabelLocation> _labelPointerAbsolute = new(); // Target is two bytes

    public Assembler(string code)
    {
        // Normalise line endings
        code = code.Replace("\r\n", "\n");
        code = code.Replace("\r", "\n");

        _tokenizer = new Tokenizer(code, _errors);
    }

    public byte[] Assemble()
    {
        _tokens = _tokenizer.Tokenize();
        AssembleToBinary();
        InsertLabels();
        return _assembledCode.ToArray();
    }

    private void AssembleToBinary()
    {
        if (_tokens.Count == 0)
        {
            return;
        }

        while (Peek() is not null)
        {
            AssembleLine();
        }
    }

    #region AssembleToBinary

    private void AddLabelSource(string label)
    {
        _labelSources.Add(label, _assembledCode.Count - 1);
    }
    
    private void AddLabelPointerRelative(string label)
    {
        _labelPointerRelative.Add(new LabelLocation(label, _assembledCode.Count - 1));
    }
    
    private void AddLabelPointerAbsolute(string label)
    {
        _labelPointerAbsolute.Add(new LabelLocation(label, _assembledCode.Count - 1));
    }

    private IToken? Peek()
    {
        return _cursor < _tokens.Count ? _tokens[_cursor] : null;
    }

    private IToken? Take()
    {
        IToken? token = Peek();
        Consume();
        return token;
    }

    private void Consume()
    {
        ++_cursor;
    }

    private void ConsumeTokenLine()
    {
        while (Peek() is not NewLineToken and not null)
        {
            Consume();
        }
    }

    private void AssembleLine()
    {
        switch (Take())
        {
            case LabelToken token:
                AddLabelSource(token.Label);
                break;
            case VariableToken token:
                int? value = ResolveMath();
                if (value is null) { TokenError(token); return; }
                _variables.Add(token.Label, value.Value);
                break;
            case TextToken token:
                AssembleInstruction(token);
                break;
            case NewLineToken or null: break;
            case { } token:
                TokenError(token);
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

    private int? ResolveMath()
    {
        // TODO add actual maths
        int? result;
        switch (Peek())
        {
            case VariableToken variableToken:
                result = (_variables.TryGetValue(variableToken.Label, out int value) ? value : null); break;
            case IntegerToken integerToken: result = integerToken.Integer; break;
            case MinusToken: Consume();
                if (Peek() is IntegerToken token)
                {
                    result = -token.Integer; break;
                }
                result = null; break;
            
            default: result = null; break;
        }
        if (result is not null)
        {
            Consume();
        }
        return result;
    }

    private void AssembleInstruction(TextToken token)
    {
        switch (token.Text.Length)
        {
            case 2: Assemble2CharInstruction(token.Text);
                break;
            case 3: Assemble3CharInstruction(token.Text);
                break;
            case 4: Assemble4CharInstruction(token.Text);
                break;
            default:
                TokenError(token);
                break;
        }
    }

    private void Assemble2CharInstruction(string instruction)
    {
        IToken? nextToken = Peek();
        switch (instruction)
        {
            case "ld": AssembleLd(nextToken); break;
            case "ex": AssembleEx(nextToken); break;
            case "jr": AssembleJr(nextToken); break;
            case "or": AssembleOr(nextToken); break;
            case "cp": AssembleCp(nextToken); break;
            case "jp": AssembleJp(nextToken); break;
            case "in": AssembleIn(nextToken); break;
            case "di": WriteByte(0xF3); break;
            case "ei": WriteByte(0xFB); break;
            case "rl": AssembleRl(nextToken); break;
            case "rr": AssembleRr(nextToken); break;
            case "im": AssembleIm(nextToken); break;
        }
        AssertEndOfLine(Peek());
    }

    #region Assemble 'ld'

    private void AssembleLd(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken textToken: AssembleLdRegister(textToken); return;
            case LBracketToken: AssembleLdAddress(); return;
        }
        TokenError(nextToken);
    }

    private void AssembleLdRegister(TextToken textToken)
    {
        switch (textToken.Text.Length)
        {
            case 1: AssembleLd1ByteRegister(textToken); return;
            case 2: AssembleLd2ByteRegister(textToken); return;
        }
        TokenError(textToken);
    }

    private void AssembleLd1ByteRegister(TextToken textToken)
    {
        Consume();
        switch (textToken)
        {
            case { Text: "b" }:
                if (AssembleLd1ByteRegisterInstruction(0x40, 0x46)) { return; }
                break;
            case { Text: "c" }:
                if (AssembleLd1ByteRegisterInstruction(0x48, 0x4E)) { return; }
                break;
            case { Text: "d" }:
                if (AssembleLd1ByteRegisterInstruction(0x50, 0x56)) { return; }
                break;
            case { Text: "e" }:
                if (AssembleLd1ByteRegisterInstruction(0x58, 0x5E)) { return; }
                break;
            case { Text: "h" }:
                if (AssembleLd1ByteRegisterInstruction(0x60, 0x66)) { return; }
                break;
            case { Text: "l" }:
                if (AssembleLd1ByteRegisterInstruction(0x68, 0x6E)) { return; }
                break;
            case { Text: "a" }:
                if (AssembleLd1ByteRegisterAInstruction(0x78, 0x7E)) { return; }
                break;
            case { Text: "ixh" }: WriteByte(0xDD);
                if (AssembleLd1ByteRegisterInstruction(0x60, 0x66)) { return; }
                break;
            case { Text: "ixl" }: WriteByte(0xDD);
                if (AssembleLd1ByteRegisterInstruction(0x68, 0x6E)) { return; }
                break;
            case { Text: "iyh" }: WriteByte(0xFD);
                if (AssembleLd1ByteRegisterInstruction(0x60, 0x66)) { return; }
                break;
            case { Text: "iyl" }: WriteByte(0xFD);
                if (AssembleLd1ByteRegisterInstruction(0x68, 0x6E)) { return; }
                break;
        }
        TokenError(textToken);
    }

    private bool AssembleLd1ByteRegisterAInstruction(byte immediateInstruction, byte addressInstruction)
    {
        if (!AssertNextToken<CommaToken>()) { return false; } Consume();
        if (Peek() is LBracketToken)
        {
            Consume();
            switch (Peek())
            {
                case TextToken { Text: "bc" }: Consume(); WriteByte(0x0A);
                    if (Peek() is not RBracketToken) { return false; } Consume(); return true;
                case TextToken { Text: "de" }: Consume(); WriteByte(0x0A);
                    if (Peek() is not RBracketToken) { return false; } Consume(); return true;
            }
            int? address = ResolveMath();
            if (address is null)
            {
                return AssembleLd1ByteRegisterFromAddress(addressInstruction);
            }
            WriteByte(0x3A);
            WriteAddress((ushort)address);
            if (Peek() is not RBracketToken) { return false; } Consume(); return true;
            
        }
        return AssembleLd1ByteRegisterFromImmediate(immediateInstruction);
    }

    private bool AssembleLd1ByteRegisterInstruction(byte immediateInstruction, byte addressInstruction)
    {
        if (!AssertNextToken<CommaToken>()) { return false; } Consume();
        if (Peek() is LBracketToken)
        {
            Consume();
            return AssembleLd1ByteRegisterFromAddress(addressInstruction);
        }
        return AssembleLd1ByteRegisterFromImmediate(immediateInstruction);
    }

    private bool AssembleLd1ByteRegisterFromAddress(byte instruction)
    {
        bool offset = false;
        switch (Peek())
        {
            case TextToken { Text: "hl" }:
                Consume();
                if (!AssertNextToken<RBracketToken>()) { return false; } Consume();
                WriteByte(instruction);
                return true;
            case TextToken { Text: "ix" }:
                Consume(); WriteByte(0xDD);
                offset = true; break;
            case TextToken { Text: "iy" }:
                Consume(); WriteByte(0xFD);
                offset = true; break;
        }
        if (!offset) return false;
        
        if (!AssertNextToken<PlusToken>()) { return false; } Consume();
        int? address = ResolveMath();
        if (address is null || !AssertNextToken<RBracketToken>()) { return false; } Consume();
        WriteByte(0xED); WriteByte(instruction);
        WriteAddress((ushort)address);
        return true;

    }

    private bool AssembleLd1ByteRegisterFromImmediate(byte baseInstruction)
    {
        switch (Peek())
        {
            case TextToken { Text: "b" }: Consume(); WriteByte(baseInstruction); return true;
            case TextToken { Text: "c" }: Consume(); WriteByte((byte)(baseInstruction + 1)); return true;
            case TextToken { Text: "d" }: Consume(); WriteByte((byte)(baseInstruction + 2)); return true;
            case TextToken { Text: "e" }: Consume(); WriteByte((byte)(baseInstruction + 3)); return true;
            case TextToken { Text: "h" }: Consume(); WriteByte((byte)(baseInstruction + 4)); return true;
            case TextToken { Text: "l" }: Consume(); WriteByte((byte)(baseInstruction + 5)); return true;
            case TextToken { Text: "a" }: Consume(); WriteByte((byte)(baseInstruction + 7)); return true;
            case TextToken { Text: "ixl" }: Consume(); WriteByte(0xDD); WriteByte((byte)(baseInstruction + 4)); return true;
            case TextToken { Text: "ixa" }: Consume(); WriteByte(0xDD); WriteByte((byte)(baseInstruction + 5)); return true;
            case TextToken { Text: "iyl" }: Consume(); WriteByte(0xFD); WriteByte((byte)(baseInstruction + 4)); return true;
            case TextToken { Text: "iya" }: Consume(); WriteByte(0xFD); WriteByte((byte)(baseInstruction + 5)); return true;
        }

        int? value = ResolveMath();
        if (value is null) { return false; }
        WriteByte((byte)(baseInstruction - 0x3A));
        WriteByte((byte)value);
        return true;
    }

    private void AssembleLd2ByteRegister(TextToken textToken)
    {
        Consume();
        switch (textToken)
        {
            case { Text: "bc" }:
                if (!AssertNextToken<CommaToken>()) { return; } Consume();

                if (Peek() is LBracketToken)
                {
                    Consume();
                    if (AssembleLd2ByteRegisterFromAddress(0x4B)) { return; }
                    break;
                }
                if (AssembleLd2ByteRegisterFromImmediate(0x01)) { return; }
                break;
            case { Text: "de" }:
                if (!AssertNextToken<CommaToken>()) { return; } Consume();

                if (Peek() is LBracketToken)
                {
                    Consume();
                    if (AssembleLd2ByteRegisterFromAddress(0x5B)) { return; }
                    break;
                }
                if (AssembleLd2ByteRegisterFromImmediate(0x11)) { return; }
                break;
            case { Text: "hl" }:
                if (!AssertNextToken<CommaToken>()) { return; } Consume();

                if (Peek() is LBracketToken)
                {
                    Consume();
                    if (AssembleLd2ByteRegisterFromAddress(0x6B)) { return; }
                    break;
                }
                if (AssembleLd2ByteRegisterFromImmediate(0x21)) { return; }
                break;
            case { Text: "sp" }:
                if (!AssertNextToken<CommaToken>()) { return; } Consume();

                if (Peek() is LBracketToken)
                {
                    Consume();
                    if (AssembleLd2ByteRegisterFromAddress(0x7B)) { return; }
                    break;
                }
                if (AssembleLd2ByteRegisterFromImmediate(0x31)) { return; }
                break;
        }
        TokenError(textToken);
    }

    private bool AssembleLd2ByteRegisterFromAddress(byte instruction)
    {
        int? address = ResolveMath();
        if (address is null || !AssertNextToken<RBracketToken>()) { return false; }
        Consume();
        WriteByte(0xED); WriteByte(instruction);
        WriteAddress((ushort)address);
        return true;
    }

    private bool AssembleLd2ByteRegisterFromImmediate(byte instruction)
    {
        int? value = ResolveMath();
        if (value is null) { return false; }
        WriteByte(instruction);
        WriteAddress((ushort)value);
        return true;
    }
    
    private void AssembleLdAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        switch (nextToken)
        {
            case TextToken { Text: "bc" }: 
                Consume();
                if (!AssertNextToken<RBracketToken>()) { return; } Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                if (Peek() is TextToken { Text: "a" })
                {
                    WriteByte(0x02); Consume(); return;
                }
                break;
            case TextToken { Text: "de" }: 
                Consume();
                if (!AssertNextToken<RBracketToken>()) { return; } Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                if (Peek() is TextToken { Text: "a" })
                {
                    WriteByte(0x12); Consume(); return;
                }
                break;
            case TextToken { Text: "hl" }: AssembleLdAddressHl(); return;
            case TextToken { Text: "ix" }:
                WriteByte(0xDD); break;
            case TextToken { Text: "iy" }:
                WriteByte(0xFD); break;
            default:
                AssembleLdAddressImmediate(nextToken); return;

        }

        Consume();
        if (!AssertNextToken<PlusToken>()) { return; } Consume();
        int? offset = ResolveMath();
        if (offset is null) { TokenError(nextToken); return; } Consume();
        if (!AssertNextToken<RBracketToken>()) { return; } Consume();
        if (!AssertNextToken<CommaToken>()) { return; } Consume();
        switch (Peek())
        {
            case TextToken { Text: "b" }: Consume(); WriteByte(0x70); WriteByte((byte)offset); return;
            case TextToken { Text: "c" }: Consume(); WriteByte(0x71); WriteByte((byte)offset); return;
            case TextToken { Text: "d" }: Consume(); WriteByte(0x72); WriteByte((byte)offset); return;
            case TextToken { Text: "e" }: Consume(); WriteByte(0x73); WriteByte((byte)offset); return;
            case TextToken { Text: "h" }: Consume(); WriteByte(0x74); WriteByte((byte)offset); return;
            case TextToken { Text: "l" }: Consume(); WriteByte(0x75); WriteByte((byte)offset); return;
            case TextToken { Text: "a" }: Consume(); WriteByte(0x77); WriteByte((byte)offset); return;
            default:
                int? value = ResolveMath();
                if (value is null) { TokenError(nextToken); return; }
                WriteByte(0x76);
                WriteByte((byte)offset);
                WriteByte((byte)value);
                return;
        }
    }

    private void AssembleLdAddressHl()
    {
        Consume();
        if (!AssertNextToken<RBracketToken>()) { return; } Consume();
        if (!AssertNextToken<CommaToken>()) { return; } Consume();
        IToken? nextToken = Peek();
        switch (nextToken)
        {
            case TextToken { Text: "b" }: WriteByte(0x70); return;
            case TextToken { Text: "c" }: WriteByte(0x71); return;
            case TextToken { Text: "d" }: WriteByte(0x72); return;
            case TextToken { Text: "e" }: WriteByte(0x73); return;
            case TextToken { Text: "h" }: WriteByte(0x74); return;
            case TextToken { Text: "l" }: WriteByte(0x75); return;
            case TextToken { Text: "a" }: WriteByte(0x77); return;
            default:
                int? value = ResolveMath();
                if (value is null) { TokenError(nextToken); return; }
                WriteByte(0x36);
                WriteByte((byte)value);
                return;
        }
    }

    private void AssembleLdAddressImmediate(IToken? token)
    {
        int? value = ResolveMath();
        if (value is null) { TokenError(token); return; }
        if (!AssertNextToken<RBracketToken>()) { return; } Consume();
        if (!AssertNextToken<CommaToken>()) { return; } Consume();
        
        IToken? nextToken = Peek();
        switch (nextToken)
        {
            case TextToken { Text: "hl" }: Consume(); WriteByte(0x22); WriteByte((byte)value); return;
            case TextToken { Text: "a" }: Consume(); WriteByte(0x32); WriteByte((byte)value); return;
            case TextToken { Text: "bc" }: Consume(); WriteByte(0xED); WriteByte(0x43); WriteByte((byte)value); return;
            case TextToken { Text: "de" }: Consume(); WriteByte(0xED); WriteByte(0x53); WriteByte((byte)value); return;
            case TextToken { Text: "sp" }: Consume(); WriteByte(0xED); WriteByte(0x73); WriteByte((byte)value); return;
            case TextToken { Text: "ix" }: Consume(); WriteByte(0xDD); WriteByte(0x22); WriteByte((byte)value); return;
            case TextToken { Text: "iy" }: Consume(); WriteByte(0xFD); WriteByte(0x22); WriteByte((byte)value); return;
        }
        TokenError(token);
    }

    #endregion

    #region Assemble 'ex'

    private void AssembleEx(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken { Text: "af" }:
                Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                if (Peek() is not TextToken { Text: "af'" }) { TokenError(nextToken); return; } Consume();
                WriteByte(0x08); return;
            case TextToken { Text: "de" }:
                Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                if (Peek() is not TextToken { Text: "hl" }) { TokenError(nextToken); return; } Consume();
                WriteByte(0xEB); return;
            case LBracketToken:
                Consume();
                if (Peek() is not TextToken { Text: "sp" }) { TokenError(nextToken); return; } Consume();
                if (!AssertNextToken<RBracketToken>()) { return; } Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                switch (Peek())
                {
                    case TextToken { Text: "hl" }: WriteByte(0xE3); return;
                    case TextToken { Text: "ix" }: WriteByte(0xDD); WriteByte(0xE3); return;
                    case TextToken { Text: "iy" }: WriteByte(0xFD); WriteByte(0xE3); return;
                }
                break;
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'jr'

    private void AssembleJr(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken { Text: "nz" }:
                Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                WriteByte(0x20); if (AssembleJrOffset()) return;
                TokenError(nextToken); return;
            case TextToken { Text: "z" }:
                Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                WriteByte(0x28); if (AssembleJrOffset()) return;
                TokenError(nextToken); return;
            case TextToken { Text: "nc" }:
                Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                WriteByte(0x30); if (AssembleJrOffset()) return;
                TokenError(nextToken); return;
            case TextToken { Text: "c" }:
                Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                WriteByte(0x38); if (AssembleJrOffset()) return;
                TokenError(nextToken); return;
            default: WriteByte(0x18); if (AssembleJrOffset()) return;
                TokenError(nextToken); return;
        }
    }

    private bool AssembleJrOffset()
    {
        IToken? offsetToken = Peek();
        if (offsetToken is TextToken textToken)
        {
            AddRelativeAddressLabel(textToken);
            return true;
        }
        int? value = ResolveMath();
        if (value is null) { return false;  }
        WriteByte((byte)(value - 2));
        return true;
    }

    #endregion

    #region Assemble 'or'

    private void AssembleOr(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken textToken: AssembleOrToARegister(textToken); return;
            case LBracketToken: AssembleOrToAAddress(); return;
        }
        WriteByte(0xF6);
        int? value = ResolveMath();
        if (value is null) { TokenError(nextToken); return; }
        WriteByte((byte)value);
    }

    private void AssembleOrToARegister(TextToken nextToken)
    {
        Consume();
        switch (nextToken)
        {
            case { Text: "b" }: WriteByte(0xB0); return;
            case { Text: "c" }: WriteByte(0xB1); return;
            case { Text: "d" }: WriteByte(0xB2); return;
            case { Text: "e" }: WriteByte(0xB3); return;
            case { Text: "h" }: WriteByte(0xB4); return;
            case { Text: "l" }: WriteByte(0xB5); return;
            case { Text: "a" }: WriteByte(0xB7); return;
            case { Text: "ixh" }: WriteByte(0xDD); WriteByte(0xB4); return;
            case { Text: "ixl" }: WriteByte(0xFD); WriteByte(0xB5); return;
        }
        TokenError(nextToken);
    }

    private void AssembleOrToAAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }:
                Consume();
                WriteByte(0xB6);
                if (!AssertNextToken<RBracketToken>()) { return; } Consume();
                return;
            case TextToken { Text: "ix" }:
                Consume();
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }:
                Consume();
                WriteByte(0xFD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0xB6);
            if (!AssertNextToken<PlusToken>()) { return; } Consume();
            
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return;  }
            WriteByte((byte)value);
            return;
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'cp'

    private void AssembleCp(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken textToken: AssembleCpToARegister(textToken); return;
            case LBracketToken: AssembleCpToAAddress(); return;
        }
        WriteByte(0xfE);
        int? value = ResolveMath();
        if (value is null) { TokenError(nextToken); return; }
        WriteByte((byte)value);
    }

    private void AssembleCpToARegister(TextToken nextToken)
    {
        Consume();
        string tokenString = nextToken.Text;
        int? command = tokenString switch
        {
            "b" => 0xB8, "c" => 0xB9, "d" => 0xBA, "e" => 0xBB, "h" => 0xBC,
            "l" => 0xBD, "a" => 0xBF, "ixh" => 0xBC, "ixl" => 0xBD,
            _ => null
        };
        if (command is null) { TokenError(nextToken); return; }
        switch (tokenString)
        {
            case [_, _, 'h']: WriteByte(0xDD); break;
            case [_, _, 'l']: WriteByte(0xFD); break;
        }
        WriteByte((byte)command.Value);
    }

    private void AssembleCpToAAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }: Consume();
                WriteByte(0xBE);
                if (!AssertNextToken<RBracketToken>()) { return; }
                Consume(); return;
            case TextToken { Text: "ix" }: Consume();
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }: Consume();
                WriteByte(0xFD); offset = true; break;
        }

        if (offset)
        {
            WriteByte(0xBE);
            if (!AssertNextToken<PlusToken>()) { return; }
            Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return;  }
            WriteByte((byte)value);
            return;
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'jp'

    private void AssembleJp(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken { Text: "nz" }: Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                WriteByte(0xC2); if (AssembleJpAddress()) return;
                TokenError(nextToken); return;
            case TextToken { Text: "z" }: Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                WriteByte(0xCA); if (AssembleJpAddress()) return;
                TokenError(nextToken); return;
            case TextToken { Text: "nc" }: Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                WriteByte(0xD2); if (AssembleJpAddress()) return;
                TokenError(nextToken); return;
            case TextToken { Text: "c" }: Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                WriteByte(0xDA); if (AssembleJpAddress()) return;
                TokenError(nextToken); return;
            case TextToken { Text: "po" }: Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                WriteByte(0xE2); if (AssembleJpAddress()) return;
                TokenError(nextToken); return;
            case TextToken { Text: "pe" }: Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                WriteByte(0xEA); if (AssembleJpAddress()) return;
                TokenError(nextToken); return;
            case TextToken { Text: "p" }: Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                WriteByte(0xF2); if (AssembleJpAddress()) return;
                TokenError(nextToken); return;
            case TextToken { Text: "m" }: Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                WriteByte(0xFA); if (AssembleJpAddress()) return;
                TokenError(nextToken); return;
            case LBracketToken:
                Consume();
                switch (Peek())
                {
                    case TextToken { Text: "hl" }: WriteByte(0xE9); break;
                    case TextToken { Text: "ix" }: WriteByte(0xDD); WriteByte(0xE9); break;
                    case TextToken { Text: "iy" }: WriteByte(0xFD); WriteByte(0xE9); break;
                }
                if (!AssertNextToken<RBracketToken>()) { return; } Consume();
                return;
            default: WriteByte(0xC3); if (AssembleJpAddress()) return;
                TokenError(nextToken); return;
                
        }
    }

    private bool AssembleJpAddress()
    {
        IToken? offsetToken = Peek();
        if (offsetToken is TextToken textToken)
        {
            AddAddressLabel(textToken);
            return true;
        }
        int? value = ResolveMath();
        if (value is null) { return false;  }
        WriteAddress((ushort)value);
        return true;
    }

    #endregion

    #region Assemble 'in'

    private void AssembleIn(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken { Text: "b" }:
                Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                if (!AssertNextToken<LBracketToken>()) { return; } Consume();
                if (Peek() is not TextToken { Text: "c" }) { TokenError(nextToken); return; } Consume();
                WriteByte(0xED); WriteByte(0x40);
                if (!AssertNextToken<RBracketToken>()) { return; } Consume(); return;
            case TextToken { Text: "c" }:
                Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                if (!AssertNextToken<LBracketToken>()) { return; } Consume();
                if (Peek() is not TextToken { Text: "c" }) { TokenError(nextToken); return; } Consume();
                WriteByte(0xED); WriteByte(0x48);
                if (!AssertNextToken<RBracketToken>()) { return; } Consume(); return;
            case TextToken { Text: "d" }:
                Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                if (!AssertNextToken<LBracketToken>()) { return; } Consume();
                if (Peek() is not TextToken { Text: "c" }) { TokenError(nextToken); return; } Consume();
                WriteByte(0xED); WriteByte(0x50);
                if (!AssertNextToken<RBracketToken>()) { return; } Consume(); return;
            case TextToken { Text: "e" }:
                Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                if (!AssertNextToken<LBracketToken>()) { return; } Consume();
                if (Peek() is not TextToken { Text: "c" }) { TokenError(nextToken); return; } Consume();
                WriteByte(0xED); WriteByte(0x58);
                if (!AssertNextToken<RBracketToken>()) { return; } Consume(); return;
            case TextToken { Text: "h" }:
                Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                if (!AssertNextToken<LBracketToken>()) { return; } Consume();
                if (Peek() is not TextToken { Text: "c" }) { TokenError(nextToken); return; } Consume();
                WriteByte(0xED); WriteByte(0x60); 
                if (!AssertNextToken<RBracketToken>()) { return; } Consume(); return;
            case TextToken { Text: "l" }:
                Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                if (!AssertNextToken<LBracketToken>()) { return; } Consume();
                if (Peek() is not TextToken { Text: "c" }) { TokenError(nextToken); return; } Consume();
                WriteByte(0xED); WriteByte(0x68);
                if (!AssertNextToken<RBracketToken>()) { return; } Consume(); return;
            case TextToken { Text: "a" }:
                Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                if (!AssertNextToken<LBracketToken>()) { return; } Consume();
                switch (Peek())
                {
                    case TextToken { Text: "a" }:
                        WriteByte(0xED); WriteByte(0x78); if (!AssertNextToken<RBracketToken>()) { return; } Consume(); return;
                    case IntegerToken integerToken:
                        WriteByte(0xDB); WriteByte((byte)integerToken.Integer); if (!AssertNextToken<RBracketToken>()) { return; } Consume(); return;
                }
                break;
            case LBracketToken:
                Consume();
                if (Peek() is not TextToken { Text: "c" }) { TokenError(nextToken); return; } Consume();
                if (!AssertNextToken<RBracketToken>()) { return; } Consume();
                WriteByte(0xED); WriteByte(0x70); return;
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'rl'

    private void AssembleRl(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken textToken: AssembleRlRegister(textToken); return;
            case LBracketToken: AssembleRlAddress(); return;
        }
        TokenError(nextToken);
    }

    private void AssembleRlRegister(TextToken token)
    {
        Consume();
        WriteByte(0xCB);
        switch (token)
        {
            case { Text: "b" }: WriteByte(0x10); return;
            case { Text: "c" }: WriteByte(0x11); return;
            case { Text: "d" }: WriteByte(0x12); return;
            case { Text: "e" }: WriteByte(0x13); return;
            case { Text: "h" }: WriteByte(0x14); return;
            case { Text: "l" }: WriteByte(0x15); return;
            case { Text: "a" }: WriteByte(0x17); return;
        }
        TokenError(token);
    }

    private void AssembleRlAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }: Consume();
                WriteByte(0xCB); WriteByte(0x16);
                if (AssertNextToken<RBracketToken>()) { Consume(); } return;
            case TextToken { Text: "ix" }: Consume();
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }: Consume();
                WriteByte(0xFD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0xCB);
            if (!AssertNextToken<PlusToken>()) { return; } Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return; }
            WriteByte((byte)value);
            if (!AssertNextToken<RBracketToken>()) { return; } Consume();
            if (Peek() is CommaToken)
            {
                Consume();
                switch (Peek())
                {
                    case TextToken { Text: "b" }: Consume(); WriteByte(0x10); return;
                    case TextToken { Text: "c" }: Consume(); WriteByte(0x11); return;
                    case TextToken { Text: "d" }: Consume(); WriteByte(0x12); return;
                    case TextToken { Text: "e" }: Consume(); WriteByte(0x13); return;
                    case TextToken { Text: "h" }: Consume(); WriteByte(0x14); return;
                    case TextToken { Text: "l" }: Consume(); WriteByte(0x15); return;
                    case TextToken { Text: "a" }: Consume(); WriteByte(0x17); return;
                }
            }
            else { WriteByte(0x16); return; }
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'rr'

    private void AssembleRr(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken textToken: AssembleRrRegister(textToken); return;
            case LBracketToken: AssembleRrAddress(); return;
        }
        TokenError(nextToken);
    }

    private void AssembleRrRegister(TextToken token)
    {
        Consume();
        WriteByte(0xCB);
        switch (token)
        {
            case { Text: "b" }: WriteByte(0x18); return;
            case { Text: "c" }: WriteByte(0x19); return;
            case { Text: "d" }: WriteByte(0x1A); return;
            case { Text: "e" }: WriteByte(0x1B); return;
            case { Text: "h" }: WriteByte(0x1C); return;
            case { Text: "l" }: WriteByte(0x1D); return;
            case { Text: "a" }: WriteByte(0x1F); return;
        }
        TokenError(token);
    }

    private void AssembleRrAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }: Consume();
                WriteByte(0xCB); WriteByte(0x1E);
                if (AssertNextToken<RBracketToken>()) { Consume(); } return;
            case TextToken { Text: "ix" }: Consume();
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }: Consume();
                WriteByte(0xFD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0xCB);
            if (!AssertNextToken<PlusToken>()) { return; } Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return; }
            WriteByte((byte)value);
            if (!AssertNextToken<RBracketToken>()) { return; } Consume();
            if (Peek() is CommaToken)
            {
                Consume();
                switch (Peek())
                {
                    case TextToken { Text: "b" }: Consume(); WriteByte(0x18); return;
                    case TextToken { Text: "c" }: Consume(); WriteByte(0x19); return;
                    case TextToken { Text: "d" }: Consume(); WriteByte(0x1A); return;
                    case TextToken { Text: "e" }: Consume(); WriteByte(0x1B); return;
                    case TextToken { Text: "h" }: Consume(); WriteByte(0x1C); return;
                    case TextToken { Text: "l" }: Consume(); WriteByte(0x1D); return;
                    case TextToken { Text: "a" }: Consume(); WriteByte(0x1F); return;
                }
            }
            else { WriteByte(0x1E); return; }
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'im'

    private void AssembleIm(IToken? nextToken)
    {
        WriteByte(0xED);
        switch (nextToken)
        {
            case IntegerToken { Integer: 0 }: WriteByte(0x46); return;
            case IntegerToken { Integer: 1 }: WriteByte(0x56); return;
            case IntegerToken { Integer: 2 }: WriteByte(0x5E); return;
        }
        TokenError(nextToken);
    }

    #endregion

    private void Assemble3CharInstruction(string instruction)
    {
        
        IToken? nextToken = Peek();
        switch (instruction)
        {
            case "nop": WriteByte(0x00); break;
            case "inc": AssembleInc(nextToken); break;
            case "dec": AssembleDec(nextToken); break;
            case "add": AssembleAdd(nextToken); break;
            case "rla": WriteByte(0x17); break;
            case "rra": WriteByte(0x1F); break;
            case "daa": WriteByte(0x27); break;
            case "cpl": WriteByte(0x2F); break;
            case "scf": WriteByte(0x37); break;
            case "ccf": WriteByte(0x3F); break;
            case "adc": AssembleAdc(nextToken); break;
            case "sub": AssembleSub(nextToken); break;
            case "sbc": AssembleSbc(nextToken); break;
            case "and": AssembleAnd(nextToken); break;
            case "xor": AssembleXOr(nextToken); break;
            case "ret": AssembleRet(nextToken); break;
            case "pop": AssemblePop(nextToken); break;
            case "rst": AssembleRst(nextToken); break;
            case "out": AssembleOut(); break;
            case "exx": WriteByte(0xD9); break;
            case "rlc": AssembleRlc(nextToken); break;
            case "rrc": AssembleRrc(nextToken); break;
            case "sla": AssembleSla(nextToken); break;
            case "sra": AssembleSra(nextToken); break;
            case "sll": AssembleSll(nextToken); break;
            case "srl": AssembleSrl(nextToken); break;
            case "bit": AssembleBit(nextToken); break;
            case "res": AssembleRes(nextToken); break;
            case "set": AssembleSet(nextToken); break;
            case "neg": Consume(); WriteByte(0xED); WriteByte(0x44); break;
            case "rrd": Consume(); WriteByte(0xED); WriteByte(0x67); break;
            case "rld": Consume(); WriteByte(0xED); WriteByte(0x6F); break;
            case "ldi": Consume(); WriteByte(0xED); WriteByte(0xA0); break;
            case "cpi": Consume(); WriteByte(0xED); WriteByte(0xA1); break;
            case "ini": Consume(); WriteByte(0xED); WriteByte(0xA2); break;
            case "ldd": Consume(); WriteByte(0xED); WriteByte(0xA8); break;
            case "cpd": Consume(); WriteByte(0xED); WriteByte(0xA9); break;
            case "ind": Consume(); WriteByte(0xED); WriteByte(0xAA); break;
        }
        AssertEndOfLine(Peek());
    }

    #region Assemble 'inc'

    private void AssembleInc(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken registerToken: AssembleIncRegister(registerToken); return;
            case LBracketToken: AssembleIncAddress(); return;
        }
        TokenError(nextToken);
    }

    private void AssembleIncRegister(TextToken nextToken)
    {
        Consume();
        switch (nextToken)
        {
            case { Text: "bc" }: WriteByte(0x03); return;
            case { Text: "de" }: WriteByte(0x13); return;
            case { Text: "hl" }: WriteByte(0x23); return;
            case { Text: "sp" }: WriteByte(0x33); return;
            case { Text: "b" }: WriteByte(0x04); return;
            case { Text: "c" }: WriteByte(0x0C); return;
            case { Text: "d" }: WriteByte(0x14); return;
            case { Text: "e" }: WriteByte(0x1C); return;
            case { Text: "h" }: WriteByte(0x24); return;
            case { Text: "l" }: WriteByte(0x2C); return;
            case { Text: "a" }: WriteByte(0x3C); return;
            case { Text: "ix" }: WriteByte(0xDD); WriteByte(0x23); return;
            case { Text: "iy" }: WriteByte(0xFD); WriteByte(0x23); return;
            case { Text: "ixh" }: WriteByte(0xDD); WriteByte(0x24); return;
            case { Text: "ixl" }: WriteByte(0xDD); WriteByte(0x2C); return;
            case { Text: "iyh" }: WriteByte(0xFD); WriteByte(0x24); return;
            case { Text: "iyl" }: WriteByte(0xFD); WriteByte(0x2C); return;
        }
        TokenError(nextToken);
    }

    private void AssembleIncAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        switch (nextToken)
        {
            case TextToken { Text: "hl" }: Consume();
                if (!AssertNextToken<RBracketToken>()) { return; } Consume();
                WriteByte(0x34);
                return;
            case TextToken { Text: "ix" }:
                WriteByte(0xDD);
                AssembleIncOffsetAddress(nextToken); return;
            case TextToken { Text: "iy" }:
                WriteByte(0xFD);
                AssembleIncOffsetAddress(nextToken); return;
        }
        TokenError(nextToken);
    }

    private void AssembleIncOffsetAddress(IToken? nextToken)
    {
        Consume();
        WriteByte(0x34);

        if (!AssertNextToken<PlusToken>()) { return; } Consume();
        
        int? value = ResolveMath();
        if (value is null) { TokenError(nextToken); return; }
        WriteByte((byte)value);

        if (!AssertNextToken<RBracketToken>()) { return; } Consume();
    }

    #endregion

    #region Assemble 'dec'

    private void AssembleDec(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken registerToken: AssembleDecRegister(registerToken); return;
            case LBracketToken: AssembleDecAddress(); return;
        }
        TokenError(nextToken);
    }

    private void AssembleDecRegister(TextToken nextToken)
    {
        Consume();
        switch (nextToken)
        {
            case { Text: "bc" }: WriteByte(0x0B); return;
            case { Text: "de" }: WriteByte(0x1B); return;
            case { Text: "hl" }: WriteByte(0x2B); return;
            case { Text: "sp" }: WriteByte(0x3B); return;
            case { Text: "b" }: WriteByte(0x05); return;
            case { Text: "c" }: WriteByte(0x0D); return;
            case { Text: "d" }: WriteByte(0x15); return;
            case { Text: "e" }: WriteByte(0x1D); return;
            case { Text: "h" }: WriteByte(0x25); return;
            case { Text: "l" }: WriteByte(0x2D); return;
            case { Text: "a" }: WriteByte(0x3D); return;
            case { Text: "ix" }: WriteByte(0xDD); WriteByte(0x2B); return;
            case { Text: "iy" }: WriteByte(0xFD); WriteByte(0x2B); return;
            case { Text: "ixh" }: WriteByte(0xDD); WriteByte(0x25); return;
            case { Text: "ixl" }: WriteByte(0xDD); WriteByte(0x2D); return;
            case { Text: "iyh" }: WriteByte(0xFD); WriteByte(0x25); return;
            case { Text: "iyl" }: WriteByte(0xFD); WriteByte(0x2D); return;
        }
        TokenError(nextToken);
    }

    private void AssembleDecAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        switch (nextToken)
        {
            case TextToken { Text: "hl" }: Consume();
                if (!AssertNextToken<RBracketToken>()) { return; } Consume();
                WriteByte(0x35); return;
            case TextToken { Text: "ix" }: WriteByte(0xDD);
                AssembleDecOffsetAddress(nextToken); return;
            case TextToken { Text: "iy" }: WriteByte(0xFD);
                AssembleDecOffsetAddress(nextToken); return;
        }
        TokenError(nextToken);
    }

    private void AssembleDecOffsetAddress(IToken? nextToken)
    {
        Consume();
        WriteByte(0x35);

        if (!AssertNextToken<PlusToken>()) { return; } Consume();
        
        int? value = ResolveMath();
        if (value is null) { TokenError(nextToken); return; }
        WriteByte((byte)value);

        if (!AssertNextToken<RBracketToken>()) { return; } Consume();
    }

    #endregion

    #region Assemble 'add'

    private void AssembleAdd(IToken? nextToken)
    {
        bool address = false;
        switch (nextToken)
        {
            case TextToken { Text: "a" }: AssembleAddToA(); return;
            case TextToken { Text: "hl" }: address = true; break;
            case TextToken { Text: "ix" }: WriteByte(0xDD); address = true; break;
            case TextToken { Text: "iy" }: WriteByte(0xFD); address = true; break;
        }

        if (address)
        {
            Consume();
            if (!AssertNextToken<CommaToken>()) { return; } Consume();
            switch (Peek())
            {
                case TextToken { Text: "bc" }: Consume(); WriteByte(0x09); return;
                case TextToken { Text: "de" }: Consume(); WriteByte(0x19); return;
                case TextToken { Text: "hl" }: Consume(); WriteByte(0x29); return;
                case TextToken { Text: "sp" }: Consume(); WriteByte(0x39); return;
            }
        }
        TokenError(nextToken);
    }

    private void AssembleAddToA()
    {
        Consume();
        if (!AssertNextToken<CommaToken>()) { return; } Consume();

        IToken? nextToken = Peek();
        switch (nextToken)
        {
            case TextToken textToken: AssembleAddToARegister(textToken); return;
            case LBracketToken: AssembleAddToAAddress(); return;
        }
        WriteByte(0xC6);
        int? value = ResolveMath();
        if (value is null) { TokenError(nextToken); return; }
        WriteByte((byte)value);
    }

    private void AssembleAddToARegister(TextToken nextToken)
    {
        switch (nextToken)
        {
            case TextToken { Text: "b" }: Consume(); WriteByte(0x80); return;
            case TextToken { Text: "c" }: Consume(); WriteByte(0x81); return;
            case TextToken { Text: "d" }: Consume(); WriteByte(0x82); return;
            case TextToken { Text: "e" }: Consume(); WriteByte(0x83); return;
            case TextToken { Text: "h" }: Consume(); WriteByte(0x84); return;
            case TextToken { Text: "l" }: Consume(); WriteByte(0x85); return;
            case TextToken { Text: "a" }: Consume(); WriteByte(0x87); return;
            case TextToken { Text: "ixh" }: Consume(); WriteByte(0xDD); WriteByte(0x84); return;
            case TextToken { Text: "ixl" }: Consume(); WriteByte(0xFD); WriteByte(0x85); return;
        }
        TokenError(nextToken);
    }

    private void AssembleAddToAAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }: Consume();
                WriteByte(0x86);
                if (!AssertNextToken<RBracketToken>()) { return; } Consume();
                return;
            case TextToken { Text: "ix" }: Consume();
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }: Consume();
                WriteByte(0xFD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0x86);
            if (!AssertNextToken<PlusToken>()) { return; }
            Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return;  }
            WriteByte((byte)value);
            return;
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'adc'

    private void AssembleAdc(IToken? nextToken)
    {
        if (!AssertNextToken<TextToken>()) { return; } Consume();
        switch (nextToken)
        {
            case TextToken { Text: "a" }: AssembleAdcToA(); return;
            case TextToken { Text: "hl" }: 
                Consume();
                WriteByte(0xED);
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                switch (Peek())
                {
                    case TextToken { Text: "bc" }: WriteByte(0x4A); return;
                    case TextToken { Text: "de" }: WriteByte(0x5A); return;
                    case TextToken { Text: "hl" }: WriteByte(0x6A); return;
                    case TextToken { Text: "sp" }: WriteByte(0x7A); return;
                }
                break;
        }
        TokenError(nextToken);
    }

    private void AssembleAdcToA()
    {
        if (!AssertNextToken<CommaToken>()) { return; } Consume();

        IToken? nextToken = Peek();
        switch (nextToken)
        {
            case TextToken textToken: AssembleAdcToARegister(textToken); return;
            case LBracketToken: AssembleAdcToAAddress(); return;
        }
        WriteByte(0xCE);
        int? value = ResolveMath();
        if (value is null) { TokenError(nextToken); return; }
        WriteByte((byte)value);
    }

    private void AssembleAdcToARegister(TextToken nextToken)
    {
        Consume();
        switch (nextToken)
        {
            case { Text: "b" }: WriteByte(0x88); return;
            case { Text: "c" }: WriteByte(0x89); return;
            case { Text: "d" }: WriteByte(0x8A); return;
            case { Text: "e" }: WriteByte(0x8B); return;
            case { Text: "h" }: WriteByte(0x8C); return;
            case { Text: "l" }: WriteByte(0x8D); return;
            case { Text: "a" }: WriteByte(0x8F); return;
            case { Text: "ixh" }: WriteByte(0xDD); WriteByte(0x8C); return;
            case { Text: "ixl" }: WriteByte(0xDD); WriteByte(0x8D); return;
            case { Text: "iyh" }: WriteByte(0xFD); WriteByte(0x8C); return;
            case { Text: "iyl" }: WriteByte(0xFD); WriteByte(0x8D); return;
        }
        TokenError(nextToken);
    }

    private void AssembleAdcToAAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }:
                Consume();
                WriteByte(0x8E);
                if (!AssertNextToken<RBracketToken>()) { return; }
                Consume();
                return;
            case TextToken { Text: "ix" }:
                Consume();
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }:
                Consume();
                WriteByte(0xDD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0x8E);
            if (!AssertNextToken<PlusToken>()) { return; } Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return;  }
            WriteByte((byte)value);
            return;
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'sub'

    private void AssembleSub(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken textToken: AssembleSubToARegister(textToken); return;
            case LBracketToken: AssembleSubToAAddress(); return;
        }
        WriteByte(0xD6);
        int? value = ResolveMath();
        if (value is null) { TokenError(nextToken); return; }
        WriteByte((byte)value);
    }

    private void AssembleSubToARegister(TextToken nextToken)
    {
        switch (nextToken)
        {
            case { Text: "b" }: Consume(); WriteByte(0x90); return; 
            case { Text: "c" }: Consume(); WriteByte(0x91); return; 
            case { Text: "d" }: Consume(); WriteByte(0x92); return; 
            case { Text: "e" }: Consume(); WriteByte(0x93); return; 
            case { Text: "h" }: Consume(); WriteByte(0x94); return; 
            case { Text: "l" }: Consume(); WriteByte(0x95); return; 
            case { Text: "a" }: Consume(); WriteByte(0x97); return; 
            case { Text: "ixh" }: Consume(); WriteByte(0xDD); WriteByte(0x9C); return; 
            case { Text: "ixl" }: Consume(); WriteByte(0xDD); WriteByte(0x9D); return; 
            case { Text: "iyh" }: Consume(); WriteByte(0xFD); WriteByte(0x9C); return; 
            case { Text: "iyl" }: Consume(); WriteByte(0xFD); WriteByte(0x9D); return; 
        }
        TokenError(nextToken);
    }

    private void AssembleSubToAAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }:
                Consume();
                WriteByte(0x96);
                if (!AssertNextToken<RBracketToken>()) { return; } Consume();
                return;
            case TextToken { Text: "ix" }:
                Consume();
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }:
                Consume();
                WriteByte(0xFD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0x96);
            if (!AssertNextToken<PlusToken>()) { return; } Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return;  }
            WriteByte((byte)value);
            if (!AssertNextToken<RBracketToken>()) { return; } Consume();
            return;
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'sbc'

    private void AssembleSbc(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken { Text: "a" }: AssembleSbcToA(); return;
            case TextToken { Text: "hl" }:
                Consume();
                if (!AssertNextToken<CommaToken>()) { return; } Consume();
                
                WriteByte(0xED);
                switch (Peek())
                {
                    case TextToken { Text: "bc" }: Consume(); WriteByte(0x42); return;
                    case TextToken { Text: "de" }: Consume(); WriteByte(0x52); return;
                    case TextToken { Text: "hl" }: Consume(); WriteByte(0x62); return;
                    case TextToken { Text: "sp" }: Consume(); WriteByte(0x72); return;
                }
                break;
        }
        TokenError(nextToken);
    }

    private void AssembleSbcToA()
    {
        Consume();
        if (!AssertNextToken<CommaToken>()) { return; } Consume();

        IToken? nextToken = Peek();
        switch (nextToken)
        {
            case TextToken textToken: AssembleSbcToARegister(textToken); return;
            case LBracketToken: AssembleSbcToAAddress(); return;
        }
        WriteByte(0xDE);
        int? value = ResolveMath();
        if (value is null) { TokenError(nextToken); return; }
        WriteByte((byte)value);
    }

    private void AssembleSbcToARegister(TextToken nextToken)
    {
        switch (nextToken)
        {
            case { Text: "b" }: Consume(); WriteByte(0x98); return;
            case { Text: "c" }: Consume(); WriteByte(0x99); return;
            case { Text: "d" }: Consume(); WriteByte(0x9A); return;
            case { Text: "e" }: Consume(); WriteByte(0x9B); return;
            case { Text: "h" }: Consume(); WriteByte(0x9C); return;
            case { Text: "l" }: Consume(); WriteByte(0x9D); return;
            case { Text: "a" }: Consume(); WriteByte(0x9F); return;
            case { Text: "ixh" }: Consume(); WriteByte(0xDD); WriteByte(0x9C); return;
            case { Text: "ixl" }: Consume(); WriteByte(0xDD); WriteByte(0x9D); return;
            case { Text: "iyh" }: Consume(); WriteByte(0xFD); WriteByte(0x9C); return;
            case { Text: "iyl" }: Consume(); WriteByte(0xFD); WriteByte(0x9D); return;
        }
        TokenError(nextToken);
    }

    private void AssembleSbcToAAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }:
                Consume();
                WriteByte(0x9E);
                if (!AssertNextToken<RBracketToken>()) { return; } Consume();
                return;
            case TextToken { Text: "ix" }: 
                Consume();
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }: 
                Consume();
                WriteByte(0xFD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0x9E);
            if (!AssertNextToken<PlusToken>()) { return; } Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return;  }
            WriteByte((byte)value);
            return;
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'and'

    private void AssembleAnd(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken textToken: AssembleAndToARegister(textToken); return;
            case LBracketToken: AssembleAndToAAddress(); return;
        }
        WriteByte(0xE6);
        int? value = ResolveMath();
        if (value is null) { TokenError(nextToken); return; }
        WriteByte((byte)value);
    }

    private void AssembleAndToARegister(TextToken nextToken)
    {
        Consume();
        switch (nextToken)
        {
            case { Text: "b" }: WriteByte(0xA0); return;
            case { Text: "c" }: WriteByte(0xA1); return;
            case { Text: "d" }: WriteByte(0xA2); return;
            case { Text: "e" }: WriteByte(0xA3); return;
            case { Text: "h" }: WriteByte(0xA4); return;
            case { Text: "l" }: WriteByte(0xA5); return;
            case { Text: "a" }: WriteByte(0xA7); return;
            case { Text: "ixh" }: WriteByte(0xDD); WriteByte(0xA4); return;
            case { Text: "ixl" }: WriteByte(0xFD); WriteByte(0xA5); return;
        }
        TokenError(nextToken);
    }

    private void AssembleAndToAAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }:
                Consume();
                WriteByte(0xA6);
                if (!AssertNextToken<RBracketToken>()) { return; }
                Consume();
                return;
            case TextToken { Text: "ix" }:
                Consume();
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }:
                Consume();
                WriteByte(0xFD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0xA6);
            if (!AssertNextToken<PlusToken>()) { return; } Consume();
            
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return;  }
            WriteByte((byte)value);
            return;
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'xor'

    private void AssembleXOr(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken textToken: AssembleXOrToARegister(textToken); return;
            case LBracketToken: AssembleXOrToAAddress(); return;
        }
        WriteByte(0xEE);
        int? value = ResolveMath();
        if (value is null) { TokenError(nextToken); return; }
        WriteByte((byte)value);
    }

    private void AssembleXOrToARegister(TextToken nextToken)
    {
        if (!AssertNextToken<TextToken>()) { return; }
        Consume();
        
        string tokenString = ((TextToken)nextToken!).Text;
        int? command = tokenString switch
        {
            "b" => 0xA8, "c" => 0xA9, "d" => 0xAA, "e" => 0xAB, "h" => 0xAC,
            "l" => 0xAD, "a" => 0xAF, "ixh" => 0xAC, "ixl" => 0xAD,
            _ => null
        };
        if (command is null) { TokenError(nextToken); return; } Consume();

        switch (tokenString)
        {
            case [_, _, 'h']: WriteByte(0xDD); break;
            case [_, _, 'l']: WriteByte(0xFD); break;
        }
        WriteByte((byte)command.Value);
    }

    private void AssembleXOrToAAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }: Consume();
                WriteByte(0xAE);
                if (!AssertNextToken<RBracketToken>()) { return; }
                Consume(); return;
            case TextToken { Text: "ix" }: Consume();
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }: Consume();
                WriteByte(0xFD); offset = true; break;
        }

        if (offset)
        {
            WriteByte(0xAE);
            if (!AssertNextToken<PlusToken>()) { return; }
            Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return;  }
            WriteByte((byte)value);
            return;
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'ret'

    private void AssembleRet(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken { Text: "nz" }: Consume(); WriteByte(0xC0); return;
            case TextToken { Text: "z" }: Consume(); WriteByte(0xC8); return;
            case TextToken { Text: "nc" }: Consume(); WriteByte(0xD0); return;
            case TextToken { Text: "c" }: Consume(); WriteByte(0xD8); return;
            case TextToken { Text: "po" }: Consume(); WriteByte(0xE0); return;
            case TextToken { Text: "pe" }: Consume(); WriteByte(0xE8); return;
            case TextToken { Text: "p" }: Consume(); WriteByte(0xF0); return;
            case TextToken { Text: "m" }: Consume(); WriteByte(0xF8); return;
            case NewLineToken or null: Consume(); WriteByte(0xC9); return;
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'pop'

    private void AssemblePop(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken { Text: "bc" }: WriteByte(0xC1); return;
            case TextToken { Text: "de" }: WriteByte(0xD1); return;
            case TextToken { Text: "hl" }: WriteByte(0xE1); return;
            case TextToken { Text: "af" }: WriteByte(0xF1); return;
            case TextToken { Text: "ix" }: WriteByte(0xDD); WriteByte(0xE1); return;
            case TextToken { Text: "iy" }: WriteByte(0xFD); WriteByte(0xE1); return;
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'rst'

    private void AssembleRst(IToken? nextToken)
    {
        switch (nextToken)
        {
            case IntegerToken { Integer: 0x00 }: Consume(); WriteByte(0xC7); return;
            case IntegerToken { Integer: 0x08 }: Consume(); WriteByte(0xCF); return;
            case IntegerToken { Integer: 0x10 }: Consume(); WriteByte(0xD7); return;
            case IntegerToken { Integer: 0x18 }: Consume(); WriteByte(0xDF); return;
            case IntegerToken { Integer: 0x20 }: Consume(); WriteByte(0xE7); return;
            case IntegerToken { Integer: 0x28 }: Consume(); WriteByte(0xEF); return;
            case IntegerToken { Integer: 0x30 }: Consume(); WriteByte(0xF7); return;
            case IntegerToken { Integer: 0x38 }: Consume(); WriteByte(0xFF); return;
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'out'

    private void AssembleOut()
    {
        if (!AssertNextToken<LBracketToken>()) { return; }
        Consume();
        IToken? nextToken = Peek();
        switch (nextToken)
        {
            case TextToken { Text: "c" }:
                Consume();
                if (!AssertNextToken<RBracketToken>()) { return; }
                Consume();
                if (!AssertNextToken<CommaToken>()) { return; }
                Consume();
                switch (Peek())
                {
                    case TextToken { Text: "b" }: Consume(); WriteByte(0x41); return;
                    case TextToken { Text: "c" }: Consume(); WriteByte(0x49); return;
                    case TextToken { Text: "d" }: Consume(); WriteByte(0x51); return;
                    case TextToken { Text: "e" }: Consume(); WriteByte(0x59); return;
                    case TextToken { Text: "h" }: Consume(); WriteByte(0x61); return;
                    case TextToken { Text: "l" }: Consume(); WriteByte(0x69); return;
                    case TextToken { Text: "a" }: Consume(); WriteByte(0x79); return;
                    case IntegerToken { Integer: 0 }: Consume(); WriteByte(0x71); return;
                }
                break;
            case IntegerToken integerToken:
                Consume();
                WriteByte(0xD3);
                WriteByte((byte)integerToken.Integer);
                if (!AssertNextToken<RBracketToken>()) { return; }
                Consume();
                if (!AssertNextToken<CommaToken>()) { return; }
                Consume();

                if (Peek() is not TextToken { Text: "a" }) { TokenError(nextToken); return; }
                Consume();
                return;
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'rlc'

    private void AssembleRlc(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken textToken: AssembleRlcRegister(textToken); return;
            case LBracketToken: AssembleRlcAddress(); return;
        }
        TokenError(nextToken);
    }

    private void AssembleRlcRegister(TextToken token)
    {
        Consume();
        WriteByte(0xCB);
        switch (token)
        {
            case { Text: "b" }: WriteByte(0x00); return;
            case { Text: "c" }: WriteByte(0x01); return;
            case { Text: "d" }: WriteByte(0x02); return;
            case { Text: "e" }: WriteByte(0x03); return;
            case { Text: "h" }: WriteByte(0x04); return;
            case { Text: "l" }: WriteByte(0x05); return;
            case { Text: "a" }: WriteByte(0x07); return;
        }
        TokenError(token);
    }

    private void AssembleRlcAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }: Consume();
                WriteByte(0xCB); WriteByte(0x06);
                if (AssertNextToken<RBracketToken>()) { Consume(); } return;
            case TextToken { Text: "ix" }: Consume();
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }: Consume();
                WriteByte(0xFD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0xCB);
            if (!AssertNextToken<PlusToken>()) { return; } Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return; }
            WriteByte((byte)value);
            if (!AssertNextToken<RBracketToken>()) { return; } Consume();
            if (Peek() is CommaToken)
            {
                Consume();
                switch (Peek())
                {
                    case TextToken { Text: "b" }: Consume(); WriteByte(0x00); return;
                    case TextToken { Text: "c" }: Consume(); WriteByte(0x01); return;
                    case TextToken { Text: "d" }: Consume(); WriteByte(0x02); return;
                    case TextToken { Text: "e" }: Consume(); WriteByte(0x03); return;
                    case TextToken { Text: "h" }: Consume(); WriteByte(0x04); return;
                    case TextToken { Text: "l" }: Consume(); WriteByte(0x05); return;
                    case TextToken { Text: "a" }: Consume(); WriteByte(0x07); return;
                }
            }
            else { WriteByte(0x06); return; }
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'rrc'

    private void AssembleRrc(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken textToken: AssembleRrcRegister(textToken); return;
            case LBracketToken: AssembleRrcAddress(); return;
        }
        TokenError(nextToken);
    }

    private void AssembleRrcRegister(TextToken token)
    {
        Consume();
        WriteByte(0xCB);
        switch (token)
        {
            case { Text: "b" }: WriteByte(0x08); return;
            case { Text: "c" }: WriteByte(0x09); return;
            case { Text: "d" }: WriteByte(0x0A); return;
            case { Text: "e" }: WriteByte(0x0B); return;
            case { Text: "h" }: WriteByte(0x0C); return;
            case { Text: "l" }: WriteByte(0x0D); return;
            case { Text: "a" }: WriteByte(0x0F); return;
        }
        TokenError(token);
    }

    private void AssembleRrcAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }: Consume();
                WriteByte(0xCB); WriteByte(0x0E);
                if (AssertNextToken<RBracketToken>()) { Consume(); } return;
            case TextToken { Text: "ix" }: Consume();
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }: Consume();
                WriteByte(0xFD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0xCB);
            if (!AssertNextToken<PlusToken>()) { return; } Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return; }
            WriteByte((byte)value);
            if (!AssertNextToken<RBracketToken>()) { return; } Consume();
            if (Peek() is CommaToken)
            {
                Consume();
                switch (Peek())
                {
                    case TextToken { Text: "b" }: Consume(); WriteByte(0x08); return;
                    case TextToken { Text: "c" }: Consume(); WriteByte(0x09); return;
                    case TextToken { Text: "d" }: Consume(); WriteByte(0x0A); return;
                    case TextToken { Text: "e" }: Consume(); WriteByte(0x0B); return;
                    case TextToken { Text: "h" }: Consume(); WriteByte(0x0C); return;
                    case TextToken { Text: "l" }: Consume(); WriteByte(0x0D); return;
                    case TextToken { Text: "a" }: Consume(); WriteByte(0x0F); return;
                }
            }
            else { WriteByte(0x0E); return; }
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'sla'

    private void AssembleSla(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken textToken: AssembleSlaRegister(textToken); return;
            case LBracketToken: AssembleSlaAddress(); return;
        }
        TokenError(nextToken);
    }

    private void AssembleSlaRegister(TextToken token)
    {
        Consume();
        WriteByte(0xCB);
        switch (token)
        {
            case { Text: "b" }: WriteByte(0x10); return;
            case { Text: "c" }: WriteByte(0x11); return;
            case { Text: "d" }: WriteByte(0x12); return;
            case { Text: "e" }: WriteByte(0x13); return;
            case { Text: "h" }: WriteByte(0x14); return;
            case { Text: "l" }: WriteByte(0x15); return;
            case { Text: "a" }: WriteByte(0x17); return;
        }
        TokenError(token);
    }

    private void AssembleSlaAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }: Consume();
                WriteByte(0xCB); WriteByte(0x16);
                if (AssertNextToken<RBracketToken>()) { Consume(); } return;
            case TextToken { Text: "ix" }: Consume();
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }: Consume();
                WriteByte(0xFD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0xCB);
            if (!AssertNextToken<PlusToken>()) { return; } Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return; }
            WriteByte((byte)value);
            if (!AssertNextToken<RBracketToken>()) { return; } Consume();
            if (Peek() is CommaToken)
            {
                Consume();
                switch (Peek())
                {
                    case TextToken { Text: "b" }: Consume(); WriteByte(0x10); return;
                    case TextToken { Text: "c" }: Consume(); WriteByte(0x11); return;
                    case TextToken { Text: "d" }: Consume(); WriteByte(0x12); return;
                    case TextToken { Text: "e" }: Consume(); WriteByte(0x13); return;
                    case TextToken { Text: "h" }: Consume(); WriteByte(0x14); return;
                    case TextToken { Text: "l" }: Consume(); WriteByte(0x15); return;
                    case TextToken { Text: "a" }: Consume(); WriteByte(0x17); return;
                }
            }
            else { WriteByte(0x16); return; }
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'sra'

    private void AssembleSra(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken textToken: AssembleSraRegister(textToken); return;
            case LBracketToken: AssembleSraAddress(); return;
        }
        TokenError(nextToken);
    }

    private void AssembleSraRegister(TextToken token)
    {
        Consume();
        WriteByte(0xCB);
        switch (token)
        {
            case { Text: "b" }: WriteByte(0x18); return;
            case { Text: "c" }: WriteByte(0x19); return;
            case { Text: "d" }: WriteByte(0x1A); return;
            case { Text: "e" }: WriteByte(0x1B); return;
            case { Text: "h" }: WriteByte(0x1C); return;
            case { Text: "l" }: WriteByte(0x1D); return;
            case { Text: "a" }: WriteByte(0x1F); return;
        }
        TokenError(token);
    }

    private void AssembleSraAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }: Consume();
                WriteByte(0xCB); WriteByte(0x1E);
                if (AssertNextToken<RBracketToken>()) { Consume(); } return;
            case TextToken { Text: "ix" }: Consume();
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }: Consume();
                WriteByte(0xFD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0xCB);
            if (!AssertNextToken<PlusToken>()) { return; } Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return; }
            WriteByte((byte)value);
            if (!AssertNextToken<RBracketToken>()) { return; } Consume();
            if (Peek() is CommaToken)
            {
                Consume();
                switch (Peek())
                {
                    case TextToken { Text: "b" }: Consume(); WriteByte(0x18); return;
                    case TextToken { Text: "c" }: Consume(); WriteByte(0x19); return;
                    case TextToken { Text: "d" }: Consume(); WriteByte(0x1A); return;
                    case TextToken { Text: "e" }: Consume(); WriteByte(0x1B); return;
                    case TextToken { Text: "h" }: Consume(); WriteByte(0x1C); return;
                    case TextToken { Text: "l" }: Consume(); WriteByte(0x1D); return;
                    case TextToken { Text: "a" }: Consume(); WriteByte(0x1F); return;
                }
            }
            else { WriteByte(0x1E); return; }
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'sll'

    private void AssembleSll(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken textToken: AssembleSllRegister(textToken); return;
            case LBracketToken: AssembleSllAddress(); return;
        }
        TokenError(nextToken);
    }

    private void AssembleSllRegister(TextToken token)
    {
        Consume();
        WriteByte(0xCB);
        switch (token)
        {
            case { Text: "b" }: WriteByte(0x20); return;
            case { Text: "c" }: WriteByte(0x21); return;
            case { Text: "d" }: WriteByte(0x22); return;
            case { Text: "e" }: WriteByte(0x23); return;
            case { Text: "h" }: WriteByte(0x24); return;
            case { Text: "l" }: WriteByte(0x25); return;
            case { Text: "a" }: WriteByte(0x27); return;
        }
        TokenError(token);
    }

    private void AssembleSllAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }: Consume();
                WriteByte(0xCB); WriteByte(0x26);
                if (AssertNextToken<RBracketToken>()) { Consume(); } return;
            case TextToken { Text: "ix" }: Consume();
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }: Consume();
                WriteByte(0xFD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0xCB);
            if (!AssertNextToken<PlusToken>()) { return; } Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return; }
            WriteByte((byte)value);
            if (!AssertNextToken<RBracketToken>()) { return; } Consume();
            if (Peek() is CommaToken)
            {
                Consume();
                switch (Peek())
                {
                    case TextToken { Text: "b" }: Consume(); WriteByte(0x20); return;
                    case TextToken { Text: "c" }: Consume(); WriteByte(0x21); return;
                    case TextToken { Text: "d" }: Consume(); WriteByte(0x22); return;
                    case TextToken { Text: "e" }: Consume(); WriteByte(0x23); return;
                    case TextToken { Text: "h" }: Consume(); WriteByte(0x24); return;
                    case TextToken { Text: "l" }: Consume(); WriteByte(0x25); return;
                    case TextToken { Text: "a" }: Consume(); WriteByte(0x27); return;
                }
            }
            else { WriteByte(0x26); return; }
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'srl'

    private void AssembleSrl(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken textToken: AssembleSrlRegister(textToken); return;
            case LBracketToken: AssembleSrlAddress(); return;
        }
        TokenError(nextToken);
    }

    private void AssembleSrlRegister(TextToken token)
    {
        Consume();
        WriteByte(0xCB);
        switch (token)
        {
            case { Text: "b" }: WriteByte(0x28); return;
            case { Text: "c" }: WriteByte(0x29); return;
            case { Text: "d" }: WriteByte(0x2A); return;
            case { Text: "e" }: WriteByte(0x2B); return;
            case { Text: "h" }: WriteByte(0x2C); return;
            case { Text: "l" }: WriteByte(0x2D); return;
            case { Text: "a" }: WriteByte(0x2F); return;
        }
        TokenError(token);
    }

    private void AssembleSrlAddress()
    {
        Consume();
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }: Consume();
                WriteByte(0xCB); WriteByte(0x2E);
                if (AssertNextToken<RBracketToken>()) { Consume(); } return;
            case TextToken { Text: "ix" }: Consume();
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }: Consume();
                WriteByte(0xFD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0xCB);
            if (!AssertNextToken<PlusToken>()) { return; } Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return; }
            WriteByte((byte)value);
            if (!AssertNextToken<RBracketToken>()) { return; } Consume();
            if (Peek() is CommaToken)
            {
                Consume();
                switch (Peek())
                {
                    case TextToken { Text: "b" }: Consume(); WriteByte(0x28); return;
                    case TextToken { Text: "c" }: Consume(); WriteByte(0x29); return;
                    case TextToken { Text: "d" }: Consume(); WriteByte(0x2A); return;
                    case TextToken { Text: "e" }: Consume(); WriteByte(0x2B); return;
                    case TextToken { Text: "h" }: Consume(); WriteByte(0x2C); return;
                    case TextToken { Text: "l" }: Consume(); WriteByte(0x2D); return;
                    case TextToken { Text: "a" }: Consume(); WriteByte(0x2F); return;
                }
            }
            else { WriteByte(0x2E); return; }
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'bit'

    private void AssembleBit(IToken? nextToken)
    {
        if (nextToken is IntegerToken { Integer: >= 0 and < 8 } bit)
        {
            if (!AssertNextToken<CommaToken>()) { return; } Consume();
            switch (Peek())
            {
                case TextToken { Text: "b" }: WriteByte(0xCB); WriteByte((byte)(0x40 + bit.Integer * 8)); return;
                case TextToken { Text: "c" }: WriteByte(0xCB); WriteByte((byte)(0x41 + bit.Integer * 8)); return;
                case TextToken { Text: "d" }: WriteByte(0xCB); WriteByte((byte)(0x42 + bit.Integer * 8)); return;
                case TextToken { Text: "e" }: WriteByte(0xCB); WriteByte((byte)(0x43 + bit.Integer * 8)); return;
                case TextToken { Text: "h" }: WriteByte(0xCB); WriteByte((byte)(0x44 + bit.Integer * 8)); return;
                case TextToken { Text: "l" }: WriteByte(0xCB); WriteByte((byte)(0x45 + bit.Integer * 8)); return;
                case TextToken { Text: "a" }: WriteByte(0xCB); WriteByte((byte)(0x47 + bit.Integer * 8)); return;
                case LBracketToken: AssembleBitAddress(bit.Integer); return;
            }
        }
        TokenError(nextToken);
    }

    private void AssembleBitAddress(int bit)
    {
        Consume();
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }:
                WriteByte(0xCB);
                WriteByte((byte)(0x46 + bit * 8)); 
                return;
            case TextToken { Text: "ix" }:
                WriteByte(0xDD);
                offset = true;
                break;
            case TextToken { Text: "iy" }:
                WriteByte(0xFD);
                offset = true;
                break;
        }

        if (offset)
        {
            if (!AssertNextToken<PlusToken>()) { return; } Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return; }
            
            WriteByte(0xCB);
            WriteByte((byte)value);
            WriteByte((byte)(0x46 + bit * 8)); 
            if (!AssertNextToken<RBracketToken>()) { return; } Consume();
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'res'

    private void AssembleRes(IToken? nextToken)
    {
        if (nextToken is IntegerToken { Integer: >= 0 and < 8 } bit)
        {
            if (!AssertNextToken<CommaToken>()) { return; } Consume();
            switch (Peek())
            {
                case TextToken { Text: "b" }: WriteByte(0xCB); WriteByte((byte)(0x80 + bit.Integer * 8)); return;
                case TextToken { Text: "c" }: WriteByte(0xCB); WriteByte((byte)(0x81 + bit.Integer * 8)); return;
                case TextToken { Text: "d" }: WriteByte(0xCB); WriteByte((byte)(0x82 + bit.Integer * 8)); return;
                case TextToken { Text: "e" }: WriteByte(0xCB); WriteByte((byte)(0x83 + bit.Integer * 8)); return;
                case TextToken { Text: "h" }: WriteByte(0xCB); WriteByte((byte)(0x84 + bit.Integer * 8)); return;
                case TextToken { Text: "l" }: WriteByte(0xCB); WriteByte((byte)(0x85 + bit.Integer * 8)); return;
                case TextToken { Text: "a" }: WriteByte(0xCB); WriteByte((byte)(0x87 + bit.Integer * 8)); return;
                case LBracketToken: AssembleResAddress(bit.Integer); return;
            }
        }
        TokenError(nextToken);
    }

    private void AssembleResAddress(int bit)
    {
        Consume();
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }:
                WriteByte(0xCB);
                WriteByte((byte)(0x86 + bit * 8)); 
                return;
            case TextToken { Text: "ix" }:
                WriteByte(0xDD);
                offset = true;
                break;
            case TextToken { Text: "iy" }:
                WriteByte(0xFD);
                offset = true;
                break;
        }

        if (offset)
        {
            if (!AssertNextToken<PlusToken>()) { return; } Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return; }
            
            if (!AssertNextToken<RBracketToken>()) { return; } Consume();

            WriteByte(0xCB);
            WriteByte((byte)value);
            if (Peek() is CommaToken)
            {
                Consume();
                switch (Peek())
                {
                    case TextToken { Text: "b" }: WriteByte((byte)(0x80 + bit * 8)); return;
                    case TextToken { Text: "c" }: WriteByte((byte)(0x81 + bit * 8)); return;
                    case TextToken { Text: "d" }: WriteByte((byte)(0x82 + bit * 8)); return;
                    case TextToken { Text: "e" }: WriteByte((byte)(0x83 + bit * 8)); return;
                    case TextToken { Text: "h" }: WriteByte((byte)(0x84 + bit * 8)); return;
                    case TextToken { Text: "l" }: WriteByte((byte)(0x85 + bit * 8)); return;
                    case TextToken { Text: "a" }: WriteByte((byte)(0x87 + bit * 8)); return;
                }
            }
            WriteByte((byte)(0x86 + bit * 8)); 
            return;
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'set'

    private void AssembleSet(IToken? nextToken)
    {
        if (nextToken is IntegerToken { Integer: >= 0 and < 8 } bit)
        {
            if (!AssertNextToken<CommaToken>()) { return; } Consume();
            switch (Peek())
            {
                case TextToken { Text: "b" }: WriteByte(0xCB); WriteByte((byte)(0xC0 + bit.Integer * 8)); return;
                case TextToken { Text: "c" }: WriteByte(0xCB); WriteByte((byte)(0xC1 + bit.Integer * 8)); return;
                case TextToken { Text: "d" }: WriteByte(0xCB); WriteByte((byte)(0xC2 + bit.Integer * 8)); return;
                case TextToken { Text: "e" }: WriteByte(0xCB); WriteByte((byte)(0xC3 + bit.Integer * 8)); return;
                case TextToken { Text: "h" }: WriteByte(0xCB); WriteByte((byte)(0xC4 + bit.Integer * 8)); return;
                case TextToken { Text: "l" }: WriteByte(0xCB); WriteByte((byte)(0xC5 + bit.Integer * 8)); return;
                case TextToken { Text: "a" }: WriteByte(0xCB); WriteByte((byte)(0xC7 + bit.Integer * 8)); return;
                case LBracketToken: AssembleSetAddress(bit.Integer); return;
            }
        }
        TokenError(nextToken);
    }

    private void AssembleSetAddress(int bit)
    {
        Consume();
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }:
                WriteByte(0xCB);
                WriteByte((byte)(0xC6 + bit * 8)); 
                return;
            case TextToken { Text: "ix" }:
                WriteByte(0xDD);
                offset = true;
                break;
            case TextToken { Text: "iy" }:
                WriteByte(0xFD);
                offset = true;
                break;
        }

        if (offset)
        {
            if (!AssertNextToken<PlusToken>()) { return; } Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return; }
            
            if (!AssertNextToken<RBracketToken>()) { return; } Consume();

            WriteByte(0xCB);
            WriteByte((byte)value);
            if (Peek() is CommaToken)
            {
                Consume();
                switch (Peek())
                {
                    case TextToken { Text: "b" }: WriteByte((byte)(0xC0 + bit * 8)); return;
                    case TextToken { Text: "c" }: WriteByte((byte)(0xC1 + bit * 8)); return;
                    case TextToken { Text: "d" }: WriteByte((byte)(0xC2 + bit * 8)); return;
                    case TextToken { Text: "e" }: WriteByte((byte)(0xC3 + bit * 8)); return;
                    case TextToken { Text: "h" }: WriteByte((byte)(0xC4 + bit * 8)); return;
                    case TextToken { Text: "l" }: WriteByte((byte)(0xC5 + bit * 8)); return;
                    case TextToken { Text: "a" }: WriteByte((byte)(0xC7 + bit * 8)); return;
                }
            }
            WriteByte((byte)(0xC6 + bit * 8)); 
            return;
        }
        TokenError(nextToken);
    }

    #endregion

    
    private void Assemble4CharInstruction(string instruction)
    {
        IToken? nextToken = Peek();
        switch (instruction)
        {
            case "rlca": WriteByte(0x07); break;
            case "rrca": WriteByte(0x0F); break;
            case "djnz": AssembleDjnz(nextToken); break;
            case "halt": WriteByte(0x76); break;
            case "call": AssembleCall(nextToken); break;
            case "push": AssemblePush(nextToken); break;
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
        AssertEndOfLine(Peek());
    }

    #region Assemble 'djnz'

    private void AssembleDjnz(IToken? nextToken)
    {
        WriteByte(0x10);
        if (nextToken is TextToken textToken)
        {
            Consume();
            AddRelativeAddressLabel(textToken);
            return;
        }
        
        int? value = ResolveMath();
        if (value is null) { TokenError(nextToken); return;  }
        WriteByte((byte)(value - 2));
    }

    #endregion

    #region Assemble 'call'

    private void AssembleCall(IToken? nextToken)
    {
        if (nextToken is TextToken parameterToken)
        {
            Consume();
            switch (Peek())
            {
                case NewLineToken or null:
                    WriteByte(0xCD);
                    AddAddressLabel(parameterToken);
                    return;
                case CommaToken:
                    Consume();
                    break;
                case { } token: TokenError(token); return;
            }

            byte? command = parameterToken.Text switch
            {
                "nz" => 0xC4, "z" => 0xCC, "nc" => 0xD4, "c" => 0xDC,
                "po" => 0xE4, "pe" => 0xEC, "p" => 0xF4, "m" => 0xFC,
                _ => null
            };
            if (command is null) { TokenError(parameterToken); return; }
            WriteByte(command.Value);
            if (Peek() is TextToken addressToken)
            {
                AddAddressLabel(addressToken); return;
            }
            int? address = ResolveMath();
            if (address is null) { TokenError(nextToken); return;  }
            WriteAddress((ushort)address);
            return;
        }
        
        WriteByte(0xCD);
        int? value = ResolveMath();
        if (value is null) { TokenError(nextToken); return;  }
        WriteAddress((ushort)value);
    }

    #endregion

    #region Assemble 'push'

    private void AssemblePush(IToken? nextToken)
    {
        switch (nextToken)
        {
            case TextToken { Text: "bc" }: Consume(); WriteByte(0xC5); return;
            case TextToken { Text: "de" }: Consume(); WriteByte(0xD5); return;
            case TextToken { Text: "hl" }: Consume(); WriteByte(0xE5); return;
            case TextToken { Text: "af" }: Consume(); WriteByte(0xF5); return;
            case TextToken { Text: "ix" }: Consume(); WriteByte(0xDD); WriteByte(0xE5); return;
            case TextToken { Text: "iy" }: Consume(); WriteByte(0xFD); WriteByte(0xE5); return;
        }
        TokenError(nextToken);
    }

    #endregion

    private bool AssertNextToken<T>() where T : IToken
    {
        IToken? next = Peek();
        if (next is T)
        {
            return true;
        }
        TokenError(next);
        return false;
    }

    private void TokenError(IToken? token)
    {
        _errors.Add(new SyntaxError(token ?? new BadToken(_tokenizer.Line)));
        ConsumeTokenLine();
    }

    private void AssertEndOfLine(IToken? token)
    {
        if (token is NewLineToken or null) return;
        TokenError(token);
    }

    private void AddRelativeAddressLabel(TextToken addressToken)
    {
        Consume();
        AddLabelPointerRelative(addressToken.Text);
        WriteByte(0);
    }

    private void AddAddressLabel(TextToken addressToken)
    {
        AddLabelPointerAbsolute(addressToken.Text);
        WriteAddress(0);
    }

    #endregion

    private void InsertLabels()
    {
        foreach (LabelLocation labelLocation in  _labelPointerAbsolute)
        {
            if (_variables.TryGetValue(labelLocation.Label, out int value))
            {
                _assembledCode[labelLocation.CodePosition] = (byte)value;
                _assembledCode[labelLocation.CodePosition + 1] = (byte)(value >> 8);
            }
            _errors.Add(new SyntaxError(-1));
        }
        foreach (LabelLocation labelLocation in  _labelPointerRelative)
        {
            if (_variables.TryGetValue(labelLocation.Label, out int value))
            {
                _assembledCode[labelLocation.CodePosition] += (byte)value;
            }
            _errors.Add(new SyntaxError(-1));
        }
    }

    public bool Errors()
    {
        return _errors.Count != 0;
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
            Console.Write(token.ToString() + ' ');
        }
    }

    public void AddVariable(string label, int value)
    {
        _variables.Add(label, value);
    }
}