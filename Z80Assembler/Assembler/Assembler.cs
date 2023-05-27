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
    private readonly List<LabelLocation> _labelSources = new();
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

    private IToken? Peek()
    {
        return _cursor >= _tokens.Count ? _tokens[_cursor] : null;
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
        while (Peek() is not NewLineToken or null)
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

    private int? ResolveMath()
    {
        throw new NotImplementedException();
    }

    private void AssembleInstruction(TextToken token)
    {
        Consume();
        switch (token.Text.Length)
        {
            case 2: Assemble2CharInstruction(token.Text);
                break;
            case 3: Assemble3CharInstruction(token.Text);
                break;
            case 4: Assemble4CharInstruction(token.Text);
                break;
            default:
                ConsumeTokenLine();
                _errors.Add(new SyntaxError(token));
                break;
        }
    }

    private void Assemble2CharInstruction(string instruction)
    {
        IToken? nextToken = Peek();
        switch (instruction)
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
        AssertEndOfLine(Peek());
    }

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
            case "out": AssembleOut(nextToken); break;
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
                if (AssertNextToken<RBracketToken>()) { return; } Consume();
                WriteByte(0x34);
                return;
            case TextToken { Text: "ix" }:
                WriteByte(0xDD);
                AssembleIncOffsetAddress(); return;
            case TextToken { Text: "iy" }:
                WriteByte(0xFD);
                AssembleIncOffsetAddress(); return;
        }
        TokenError(nextToken);
    }

    private void AssembleIncOffsetAddress()
    {
        Consume();
        WriteByte(0x34);

        if (AssertNextToken<PlusToken>()) { return; } Consume();
        
        IToken? offsetToken = Peek();
        switch (offsetToken)
        {
            case IntegerToken integerToken: WriteByte((byte)integerToken.Integer); break;
            case VariableToken variableToken:
                if (_variables.TryGetValue(variableToken.Label, out int variableOffset))
                {
                    WriteByte((byte)variableOffset);
                    break;
                }
                goto default;
            default: TokenError(offsetToken); return;
        }

        if (AssertNextToken<RBracketToken>()) { return; } Consume();
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
                if (AssertNextToken<RBracketToken>()) { return; } Consume();
                WriteByte(0x35); return;
            case TextToken { Text: "ix" }: WriteByte(0xDD);
                AssembleDecOffsetAddress(); return;
            case TextToken { Text: "iy" }: WriteByte(0xFD);
                AssembleDecOffsetAddress(); return;
        }
        TokenError(nextToken);
    }

    private void AssembleDecOffsetAddress()
    {
        Consume();
        WriteByte(0x35);

        if (AssertNextToken<PlusToken>()) { return; } Consume();
        
        IToken? offsetToken = Peek();
        switch (offsetToken)
        {
            case IntegerToken integerToken: WriteByte((byte)integerToken.Integer); break;
            case VariableToken variableToken:
                if (_variables.TryGetValue(variableToken.Label, out int variableOffset))
                {
                    WriteByte((byte)variableOffset);
                    break;
                }
                goto default;
            default: TokenError(offsetToken); return;
        }

        if (AssertNextToken<RBracketToken>()) { return; } Consume();
    }

    #endregion

    #region Assemble 'add'

    private void AssembleAdd(IToken? nextToken)
    {
        bool address = false;
        switch (nextToken)
        {
            case TextToken { Text: "a" }: AssembleAddToA(); return;
            case TextToken { Text: "hl" }: address = true; return;
            case TextToken { Text: "ix" }: WriteByte(0xDD); address = true; return;
            case TextToken { Text: "iy" }: WriteByte(0xFD); address = true; return;
        }

        if (address)
        {
            Consume();
            if (AssertNextToken<CommaToken>()) { return; } Consume();
            switch (Peek())
            {
                case TextToken { Text: "bc" }: WriteByte(0x09); return;
                case TextToken { Text: "de" }: WriteByte(0x19); return;
                case TextToken { Text: "hl" }: WriteByte(0x29); return;
                case TextToken { Text: "sp" }: WriteByte(0x39); return;
            }
        }
        TokenError(nextToken);
    }

    private void AssembleAddToA()
    {
        Consume();
        if (AssertNextToken<CommaToken>()) { return; } Consume();

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

    private void AssembleAddToARegister(TextToken token)
    {
        if (token.Text is not "a") { TokenError(token); return; } Consume();
        if (AssertNextToken<CommaToken>()) { return; } Consume();
        
        IToken? nextToken = Peek();
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
                if (AssertNextToken<RBracketToken>()) { return; } Consume();
                return;
            case TextToken { Text: "ix" }: Consume();
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }: Consume();
                WriteByte(0xFD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0x86);
            if (AssertNextToken<PlusToken>()) { return; }
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
        if (AssertNextToken<TextToken>()) { return; } Consume();
        switch (nextToken)
        {
            case TextToken { Text: "a" }: AssembleAdcToA(); return;
            case TextToken { Text: "hl" }: 
                Consume();
                WriteByte(0xED);
                if (AssertNextToken<CommaToken>()) { return; } Consume();
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
        Consume();
        if (AssertNextToken<CommaToken>()) { return; } Consume();

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

    private void AssembleAdcToARegister(TextToken token)
    {
        if (token.Text is not "a") { TokenError(token); return; } Consume();
        if (AssertNextToken<CommaToken>()) { return; } Consume();
        
        IToken? nextToken = Peek();
        switch (nextToken)
        {
            case TextToken { Text: "b" }: WriteByte(0x88); return;
            case TextToken { Text: "c" }: WriteByte(0x89); return;
            case TextToken { Text: "d" }: WriteByte(0x8A); return;
            case TextToken { Text: "e" }: WriteByte(0x8B); return;
            case TextToken { Text: "h" }: WriteByte(0x8C); return;
            case TextToken { Text: "l" }: WriteByte(0x8D); return;
            case TextToken { Text: "a" }: WriteByte(0x8F); return;
            case TextToken { Text: "ixh" }: WriteByte(0xDD); WriteByte(0x8C); return;
            case TextToken { Text: "ixl" }: WriteByte(0xDD); WriteByte(0x8D); return;
            case TextToken { Text: "iyh" }: WriteByte(0xFD); WriteByte(0x8C); return;
            case TextToken { Text: "iyl" }: WriteByte(0xFD); WriteByte(0x8D); return;
        }
        TokenError(nextToken);
    }

    private void AssembleAdcToAAddress()
    {
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }:
                WriteByte(0x8E);
                if (AssertNextToken<RBracketToken>()) { return; }
                Consume();
                return;
            case TextToken { Text: "ix" }:
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }:
                WriteByte(0xDD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0x8E);
            if (AssertNextToken<PlusToken>()) { return; } Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return;  }
            WriteByte((byte)value);
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

    private void AssembleSubToARegister(TextToken token)
    {
        if (token.Text is not "a") { TokenError(token); return; } Consume();
        if (AssertNextToken<CommaToken>()) { return; } Consume();
        
        IToken? nextToken = Peek();
        switch (nextToken)
        {
            case TextToken { Text: "b" }: Consume(); WriteByte(0x90); return; 
            case TextToken { Text: "c" }: Consume(); WriteByte(0x91); return; 
            case TextToken { Text: "d" }: Consume(); WriteByte(0x92); return; 
            case TextToken { Text: "e" }: Consume(); WriteByte(0x93); return; 
            case TextToken { Text: "h" }: Consume(); WriteByte(0x94); return; 
            case TextToken { Text: "l" }: Consume(); WriteByte(0x95); return; 
            case TextToken { Text: "a" }: Consume(); WriteByte(0x97); return; 
            case TextToken { Text: "ixh" }: Consume(); WriteByte(0xDD); WriteByte(0x9C); return; 
            case TextToken { Text: "ixl" }: Consume(); WriteByte(0xDD); WriteByte(0x9D); return; 
            case TextToken { Text: "iyh" }: Consume(); WriteByte(0xFD); WriteByte(0x9C); return; 
            case TextToken { Text: "iyl" }: Consume(); WriteByte(0xFD); WriteByte(0x9D); return; 
        }
        TokenError(nextToken);
    }

    private void AssembleSubToAAddress()
    {
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }:
                WriteByte(0x96);
                if (AssertNextToken<RBracketToken>()) { return; } Consume();
                return;
            case TextToken { Text: "ix" }:
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }:
                WriteByte(0xFD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0x96);
            if (AssertNextToken<PlusToken>()) { return; } Consume();
            int? value = ResolveMath();
            if (value is null) { TokenError(nextToken); return;  }
            WriteByte((byte)value);
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
                if (AssertNextToken<CommaToken>()) { return; } Consume();
                
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
        if (AssertNextToken<CommaToken>()) { return; } Consume();

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

    private void AssembleSbcToARegister(TextToken token)
    {
        if (token.Text is not "a") { TokenError(token); return; } Consume();
        if (AssertNextToken<CommaToken>()) { return; } Consume();
        
        IToken? nextToken = Peek();
        switch (nextToken)
        {
            case TextToken { Text: "b" }: Consume(); WriteByte(0x98); return;
            case TextToken { Text: "c" }: Consume(); WriteByte(0x99); return;
            case TextToken { Text: "d" }: Consume(); WriteByte(0x9A); return;
            case TextToken { Text: "e" }: Consume(); WriteByte(0x9B); return;
            case TextToken { Text: "h" }: Consume(); WriteByte(0x9C); return;
            case TextToken { Text: "l" }: Consume(); WriteByte(0x9D); return;
            case TextToken { Text: "a" }: Consume(); WriteByte(0x9F); return;
            case TextToken { Text: "ixh" }: Consume(); WriteByte(0xDD); WriteByte(0x9C); return;
            case TextToken { Text: "ixl" }: Consume(); WriteByte(0xDD); WriteByte(0x9D); return;
            case TextToken { Text: "iyh" }: Consume(); WriteByte(0xFD); WriteByte(0x9C); return;
            case TextToken { Text: "iyl" }: Consume(); WriteByte(0xFD); WriteByte(0x9D); return;
        }
        TokenError(nextToken);
    }

    private void AssembleSbcToAAddress()
    {
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }: 
                WriteByte(0x9E);
                if (AssertNextToken<RBracketToken>()) { return; } Consume();
                return;
            case TextToken { Text: "ix" }: 
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }: 
                WriteByte(0xFD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0x9E);
            if (AssertNextToken<PlusToken>()) { return; } Consume();
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
            case TextToken textToken: AssembleAndToARegister(textToken); break;
            case LBracketToken: AssembleAndToAAddress(); break;
        }
        WriteByte(0xE6);
        int? value = ResolveMath();
        if (value is null) { TokenError(nextToken); return; }
        WriteByte((byte)value);
    }

    private void AssembleAndToARegister(TextToken token)
    {
        if (token.Text is not "a") { TokenError(token); return; } Consume();
        if (AssertNextToken<CommaToken>()) { return; } Consume();
        
        IToken? nextToken = Peek();
        switch (nextToken)
        {
            case TextToken { Text: "b" }: WriteByte(0xA0); return;
            case TextToken { Text: "c" }: WriteByte(0xA1); return;
            case TextToken { Text: "d" }: WriteByte(0xA2); return;
            case TextToken { Text: "e" }: WriteByte(0xA3); return;
            case TextToken { Text: "h" }: WriteByte(0xA4); return;
            case TextToken { Text: "l" }: WriteByte(0xA5); return;
            case TextToken { Text: "a" }: WriteByte(0xA7); return;
            case TextToken { Text: "ixh" }: WriteByte(0xDD); WriteByte(0xA4); return;
            case TextToken { Text: "ixl" }: WriteByte(0xFD); WriteByte(0xA5); return;
        }
        TokenError(nextToken);
    }

    private void AssembleAndToAAddress()
    {
        IToken? nextToken = Peek();
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }:
                WriteByte(0xA6);
                if (AssertNextToken<RBracketToken>()) { return; }
                Consume();
                return;
            case TextToken { Text: "ix" }:
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }:
                WriteByte(0xFD); offset = true; break;
        }
        if (offset)
        {
            WriteByte(0xA6);
            if (AssertNextToken<PlusToken>()) { return; } Consume();
            
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
            case LBracketToken: AssembleXOrToAAddress(nextToken); return;
        }
        WriteByte(0xEE);
        int? value = ResolveMath();
        if (value is null) { TokenError(nextToken); return; }
        WriteByte((byte)value);
    }

    private void AssembleXOrToARegister(TextToken token)
    {
        if (token.Text is not "a") { TokenError(token); return; }
        Consume();
        if (AssertNextToken<CommaToken>()) { return; }
        Consume();
        
        IToken? nextToken = Peek();
        if (AssertNextToken<TextToken>()) { return; }
        Consume();
        
        string tokenString = ((TextToken)nextToken!).Text;
        int? command = tokenString switch
        {
            "b" => 0xA8, "c" => 0xA9, "d" => 0xAA, "e" => 0xAB, "h" => 0xAC,
            "l" => 0xAD, "a" => 0xAF, "ixh" => 0xAC, "ixl" => 0xAD,
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

    private void AssembleXOrToAAddress(IToken? nextToken)
    {
        bool offset = false;
        switch (nextToken)
        {
            case TextToken { Text: "hl" }: Consume();
                WriteByte(0xAE);
                if (AssertNextToken<RBracketToken>()) { return; }
                Consume(); return;
            case TextToken { Text: "ix" }: Consume();
                WriteByte(0xDD); offset = true; break;
            case TextToken { Text: "iy" }: Consume();
                WriteByte(0xFD); offset = true; break;
        }

        if (offset)
        {
            WriteByte(0xAE);
            if (AssertNextToken<PlusToken>()) { return; }
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
            case IntegerToken { Integer: 0x00 }: WriteByte(0xC7); return;
            case IntegerToken { Integer: 0x08 }: WriteByte(0xCF); return;
            case IntegerToken { Integer: 0x10 }: WriteByte(0xD7); return;
            case IntegerToken { Integer: 0x18 }: WriteByte(0xDF); return;
            case IntegerToken { Integer: 0x20 }: WriteByte(0xE7); return;
            case IntegerToken { Integer: 0x28 }: WriteByte(0xEF); return;
            case IntegerToken { Integer: 0x30 }: WriteByte(0xF7); return;
            case IntegerToken { Integer: 0x38 }: WriteByte(0xFF); return;
        }
        TokenError(nextToken);
    }

    #endregion

    #region Assemble 'out'

    private void AssembleOut(IToken? nextToken)
    {
        if (AssertNextToken<LBracketToken>()) { TokenError(nextToken); return; }
        Consume();
        nextToken = Peek();
        switch (nextToken)
        {
            case TextToken { Text: "c" }:
                Consume();
                if (AssertNextToken<RBracketToken>()) { TokenError(nextToken); return; }
                Consume();
                if (AssertNextToken<CommaToken>()) { TokenError(nextToken); return; }
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
                if (AssertNextToken<RBracketToken>()) { TokenError(nextToken); return; }
                Consume();
                if (AssertNextToken<CommaToken>()) { TokenError(nextToken); return; }
                Consume();

                if (Peek() is not TextToken { Text: "a" }) { TokenError(nextToken); return; }
                Consume();
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
            if (Peek() is NewLineToken or null)
            {
                WriteByte(0xCD);
                AddAddressLabel(parameterToken);
                return;
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
            case TextToken { Text: "bc" }: WriteByte(0xC5); return;
            case TextToken { Text: "de" }: WriteByte(0xD5); return;
            case TextToken { Text: "hl" }: WriteByte(0xE5); return;
            case TextToken { Text: "af" }: WriteByte(0xF5); return;
            case TextToken { Text: "ix" }: WriteByte(0xDD); WriteByte(0xE5); return;
            case TextToken { Text: "iy" }: WriteByte(0xFD); WriteByte(0xE5); return;
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
        _errors.Add(new SyntaxError(token ?? new BadToken(_tokenizer.Line, _tokenizer.Column)));
        ConsumeTokenLine();
    }

    private void AssertEndOfLine(IToken? token)
    {
        if (token is not (NewLineToken or null)) return;
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

    public void InsertLabels()
    {
        
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
}