using Z80Assembler.Tokens;

namespace Z80Assembler.Ast;

public class Parser
{
    private List<Token> _tokens;
    private int _cursor;
    public Dictionary<string, List<INode>> Instructions = new();
    private string? _currentSection;
    private HashSet<int> Errors = new();
    public bool MissingSectionError = false;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
        Parse();
    }

    private Token? Peek() => _cursor < _tokens.Count ? _tokens[_cursor] : null;

    private void Consume() => ++_cursor;

    private Token? Next() => _cursor < _tokens.Count ? _tokens[_cursor++] : null;

    private void Parse()
    {
        while (Next() is { } token)
        {
            switch (token.Type)
            {
                case TokenType.Nop or TokenType.Ld or TokenType.Inc or TokenType.Dec or TokenType.Rlca or TokenType.Ex
                    or TokenType.Add or TokenType.Rrca or TokenType.Djnz or TokenType.Rla or TokenType.Rra or TokenType.Jr
                    or TokenType.Daa or TokenType.Cpl or TokenType.Scf or TokenType.Ccf or TokenType.Halt or TokenType.Adc
                    or TokenType.Sub or TokenType.Sbc or TokenType.And or TokenType.Xor or TokenType.Or or TokenType.Cp
                    or TokenType.Ret or TokenType.Pop or TokenType.Jp or TokenType.Call or TokenType.Push or TokenType.Rst
                    or TokenType.Exx or TokenType.In or TokenType.Di or TokenType.Ei or TokenType.Rlc or TokenType.Rrc
                    or TokenType.Rl or TokenType.Rr or TokenType.Sla or TokenType.Sra or TokenType.Sll or TokenType.Srl
                    or TokenType.Bit or TokenType.Res or TokenType.Set or TokenType.Neg or TokenType.Retn or TokenType.Im
                    or TokenType.Reti or TokenType.Rrd or TokenType.Rld or TokenType.Ldi or TokenType.Cpi or TokenType.Ini
                    or TokenType.Outi or TokenType.Ldd or TokenType.Cpd or TokenType.Ind or TokenType.Outd or TokenType.Ldir
                    or TokenType.Cpir or TokenType.Inir or TokenType.Otir or TokenType.Lddr or TokenType.Cpdr or TokenType.Indr
                    or TokenType.Otdr: ParseInstruction(token); break;
                case TokenType.Db: ParseDefineByte(token); break;
                case TokenType.Dw: ParseDefineWord(token); break;
                case TokenType.Section: ParseSectionDefine(token); break;
                case TokenType.Label:
                    AddInstruction(new LabelNode(token.StringValue!)); break;
                case TokenType.LineEnd: break;
                default:
                    Errors.Add(token.Line);
                    break;
            }
        }
    }

    private void ConsumeLine()
    {
        while (Peek() is { Type: not TokenType.LineEnd }) Consume();
    }

    private void Error(Token token)
    {
        Errors.Add(token.Line);
        ConsumeLine();
    }

    private void ParseInstruction(Token instruction)
    {
        InstructionType? instructionType = instruction.Type.ToInstruction();
        if (instructionType is null) throw new ArgumentOutOfRangeException();
        OperandNode? operand1 = null;
        OperandNode? operand2 = null;

        if (Peek() is { Type: not TokenType.LineEnd })
        {
            operand1 = ParseOperand(instruction);
            if (operand1 is null) return;
            if (Peek() is { Type: not TokenType.LineEnd })
            {
                if (Peek() is not { Type: TokenType.Comma })
                {
                    Error(instruction);
                    return;
                }
                Consume();
                operand2 = ParseOperand(instruction);
                if (operand2 is null) return;
            }
        }
        InstructionNode instructionNode = new(instructionType.Value, operand1, operand2);
        AddInstruction(instructionNode);
    }

    private OperandNode? ParseOperand(Token instruction)
    {
        Token? next = Peek();
        if (next is null) { Error(instruction); return null; }
        switch (next.Type)
        {
            case TokenType.LBracket: Consume(); return ParseAddressOperand(instruction);

            case TokenType.Nz or TokenType.Nc or TokenType.Po or TokenType.P or TokenType.Z or TokenType.Pe or TokenType.M:
                Consume(); return new OperandNode(next.Type.ToFlagCheck()!.Value);
            
            case TokenType.C: Consume(); return instruction.Type is TokenType.Ret or TokenType.Jp or TokenType.Jr or TokenType.Call
                            ? new OperandNode(FlagCheckType.C) : new OperandNode(RegisterType.C, false);

            case TokenType.A or TokenType.B or TokenType.D or TokenType.E or TokenType.H or TokenType.L
            or TokenType.Bc or TokenType.De or TokenType.Hl or TokenType.Sp or TokenType.Ix or TokenType.Iy
            or TokenType.Ixh or TokenType.Ixl or TokenType.Iyh or TokenType.Iyl:
                Consume(); return new OperandNode(next.Type.ToRegisterType()!.Value, false);

            case TokenType.Identifier: Consume(); return new OperandNode(next.StringValue!, false);

            default: return ParseNumberOperand(instruction);
        };
    }

    private OperandNode? ParseAddressOperand(Token instruction)
    {
        Consume();
        Token? next = Peek();
        if (next is null) { Error(instruction); return null; }

        OperandNode? node;
        switch (next.Type)
        {
            case TokenType.A or TokenType.B or TokenType.C or TokenType.D or TokenType.E or TokenType.H or TokenType.L
                or TokenType.Bc or TokenType.De or TokenType.Hl or TokenType.Sp or TokenType.Ix or TokenType.Iy
                or TokenType.Ixh or TokenType.Ixl or TokenType.Iyh or TokenType.Iyl: Consume();
                node = new OperandNode(next.Type.ToRegisterType()!.Value, true); break;

            case TokenType.Identifier: Consume(); node = new OperandNode(next.StringValue!, true); break;

            default: node = ParseNumberOperand(instruction); break;
        };

        if (Peek() is { Type: TokenType.RBracket }) return node;
        Error(instruction); return null;
    }

    private OperandNode? ParseNumberOperand(Token instruction) // TODO support maths
    {
        if (Peek() is { Type: TokenType.Integer } token)
        {
            Consume();
            return new OperandNode(token.Integer, true);
        }
        Error(instruction); return null;
    }

    private void ParseDefineWord(Token instruction)
    {
        List<int> definedWords = new();
        while (Peek() is { } token)
        {
            switch (token.Type)
            {
                case TokenType.Integer: Consume(); definedWords.Add(token.Integer); break; // TODO support labels here
                default: Error(instruction); return;
            }
            if (Peek() is { Type: not TokenType.LineEnd } token2)
            {
                if (token2.Type is not TokenType.Comma)
                {
                    Error(instruction); return;
                }
                Consume();
            }
            else
            {
                break;
            }
        }
        AddInstruction(new DefineWordsNode(definedWords));
    }

    private void ParseDefineByte(Token instruction)
    {
        List<(int?, string?)> definedBytes = new();
        while (Peek() is { } token)
        {
            switch (token.Type)
            {
                case TokenType.String: Consume(); definedBytes.Add((null, token.StringValue)); break;
                case TokenType.Integer: Consume(); definedBytes.Add((token.Integer, null)); break;
                default: Error(instruction); return;
            }
            if (Peek() is { Type: not TokenType.LineEnd } token2)
            {
                if (token2.Type is not TokenType.Comma)
                {
                    Error(instruction); return;
                }
                Consume();
            }
            else
            {
                break;
            }
        }
        AddInstruction(new DefineBytesNode(definedBytes));
    }

    private void ParseSectionDefine(Token instruction)
    {
        if (Peek() is { Type: TokenType.Identifier } token)
        {
            Consume();
            _currentSection = token.StringValue;
        }
        else
        {
            Error(instruction);
        }
    }

    private void AddInstruction(INode instruction)
    {
        if (_currentSection is null)
        {
            MissingSectionError = true;
            return;
        }
        if (Instructions.TryGetValue(_currentSection, out List<INode> instructions))
        {
            instructions.Add(instruction);
        } else
        {
            Instructions.Add(_currentSection, new List<INode>() { instruction });
        }
    }
}