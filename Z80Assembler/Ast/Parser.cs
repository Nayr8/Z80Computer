using Z80Assembler.Tokens;

namespace Z80Assembler.Ast;

public class Parser
{
    private List<Token> _tokens;
    private int _cursor;
    private List<InstructionNode> _instructions = new();
    private HashSet<int> _errors = new();

    public void Run(List<Token> tokens)
    {
        new Parser(tokens).Parse();
    }

    private Parser(List<Token> tokens)
    {
        _tokens = tokens;
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
                    or TokenType.Otdr or TokenType.LineEnd: ParseInstruction(token); break;
                case TokenType.Db: break;
                case TokenType.Dw: break;
                default:
                    _errors.Add(token.Line);
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
        _errors.Add(token.Line);
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
                operand2 = ParseOperand(instruction);
                if (operand2 is null) return;
            }
        }
        InstructionNode instructionNode = new(instructionType.Value, operand1, operand2);
        _instructions.Add(instructionNode);
    }

    private OperandNode? ParseOperand(Token instruction)
    {
        Token? next = Peek();
        if (next is null) { Error(instruction); return null; }
        return next.Type switch
        {
            TokenType.LBracket => ParseAddressOperand(instruction),

            TokenType.Nz or TokenType.Nc or TokenType.Po or TokenType.P or TokenType.Z or TokenType.Pe or TokenType.M =>
                new OperandNode(next.Type.ToFlagCheck()!.Value),
            
            TokenType.C => instruction.Type is TokenType.Ret or TokenType.Jp or TokenType.Jr or TokenType.Call
                            ? new OperandNode(FlagCheckType.C) : new OperandNode(RegisterType.C, false),

            TokenType.A or TokenType.B or TokenType.D or TokenType.E or TokenType.H or TokenType.L
            or TokenType.Bc or TokenType.De or TokenType.Hl or TokenType.Sp or TokenType.Ix or TokenType.Iy
            or TokenType.Ixh or TokenType.Ixl or TokenType.Iyh or TokenType.Iyl => new OperandNode(next.Type.ToRegisterType()!.Value, false),

            TokenType.Identifier => new OperandNode(next.StringValue!, false),

            _ => ParseNumberOperand(instruction)
        };
    }

    private OperandNode? ParseAddressOperand(Token instruction)
    {
        Consume();
        Token? next = Peek();
        if (next is null) { Error(instruction); return null; }

        OperandNode? node = next.Type switch
        {
            TokenType.A or TokenType.B or TokenType.C or TokenType.D or TokenType.E or TokenType.H or TokenType.L
                or TokenType.Bc or TokenType.De or TokenType.Hl or TokenType.Sp or TokenType.Ix or TokenType.Iy
                or TokenType.Ixh or TokenType.Ixl or TokenType.Iyh or TokenType.Iyl => new OperandNode(next.Type.ToRegisterType()!.Value, true),

            TokenType.Identifier => new OperandNode(next.StringValue!, true),

            _ => ParseNumberOperand(instruction)
        };

        if (Peek() is { Type: TokenType.RBracket }) return node;
        Error(instruction); return null;
    }

    private OperandNode? ParseNumberOperand(Token instruction) // TODO support maths
    {
        if (Peek() is { Type: TokenType.Integer } token)
        {
            return new OperandNode(token.Integer, true);
        }
        Error(instruction); return null;
    }
}