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
                    case TokenType.Nop: ParseInstructionNoOperands(token); break;
                    case TokenType.Ld: break;
                    case TokenType.Inc: break;
                    case TokenType.Dec: break;
                    case TokenType.Rlca: ParseInstructionNoOperands(token); break;
                    case TokenType.Ex: break;
                    case TokenType.Add: break;
                    case TokenType.Rrca: ParseInstructionNoOperands(token); break;
                    case TokenType.Djnz: break;
                    case TokenType.Rla: ParseInstructionNoOperands(token); break;
                    case TokenType.Rra: ParseInstructionNoOperands(token); break;
                    case TokenType.Jr: break;
                    case TokenType.Daa: ParseInstructionNoOperands(token); break;
                    case TokenType.Cpl: ParseInstructionNoOperands(token); break;
                    case TokenType.Scf: ParseInstructionNoOperands(token); break;
                    case TokenType.Ccf: ParseInstructionNoOperands(token); break;
                    case TokenType.Halt: ParseInstructionNoOperands(token); break;
                    case TokenType.Adc: break;
                    case TokenType.Sub: break;
                    case TokenType.Sbc: break;
                    case TokenType.And: break;
                    case TokenType.Xor: break;
                    case TokenType.Or: break;
                    case TokenType.Cp: break;
                    case TokenType.Ret: break;
                    case TokenType.Pop: break;
                    case TokenType.Jp: break;
                    case TokenType.Call: break;
                    case TokenType.Push: break;
                    case TokenType.Rst: break;
                    case TokenType.Exx: ParseInstructionNoOperands(token); break;
                    case TokenType.In: break;
                    case TokenType.Di: ParseInstructionNoOperands(token); break;
                    case TokenType.Ei: ParseInstructionNoOperands(token); break;
                    case TokenType.Rlc: break;
                    case TokenType.Rrc: break;
                    case TokenType.Rl: break;
                    case TokenType.Rr: break;
                    case TokenType.Sla: break;
                    case TokenType.Sra: break;
                    case TokenType.Sll: break;
                    case TokenType.Srl: break;
                    case TokenType.Bit: break;
                    case TokenType.Res: break;
                    case TokenType.Set: break;
                    case TokenType.Neg: ParseInstructionNoOperands(token); break;
                    case TokenType.Retn: ParseInstructionNoOperands(token); break;
                    case TokenType.Im: break;
                    case TokenType.Reti: ParseInstructionNoOperands(token); break;
                    case TokenType.Rrd: ParseInstructionNoOperands(token); break;
                    case TokenType.Rld: ParseInstructionNoOperands(token); break;
                    case TokenType.Ldi: ParseInstructionNoOperands(token); break;
                    case TokenType.Cpi: ParseInstructionNoOperands(token); break;
                    case TokenType.Ini: ParseInstructionNoOperands(token); break;
                    case TokenType.Outi: ParseInstructionNoOperands(token); break;
                    case TokenType.Ldd: ParseInstructionNoOperands(token); break;
                    case TokenType.Cpd: ParseInstructionNoOperands(token); break;
                    case TokenType.Ind: ParseInstructionNoOperands(token); break;
                    case TokenType.Outd: ParseInstructionNoOperands(token); break;
                    case TokenType.Ldir: ParseInstructionNoOperands(token); break;
                    case TokenType.Cpir: ParseInstructionNoOperands(token); break;
                    case TokenType.Inir: ParseInstructionNoOperands(token); break;
                    case TokenType.Otir: ParseInstructionNoOperands(token); break;
                    case TokenType.Lddr: ParseInstructionNoOperands(token); break;
                    case TokenType.Cpdr: ParseInstructionNoOperands(token); break;
                    case TokenType.Indr: ParseInstructionNoOperands(token); break;
                    case TokenType.Otdr: ParseInstructionNoOperands(token); break;
                    case TokenType.LineEnd: break;
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

    private void ParseInstructionNoOperands(Token instructionToken)
    {
        if (Next() is { Type: not TokenType.LineEnd } token)
        {
            _errors.Add(token.Line);
            ConsumeLine();
            return;
        }

        InstructionType? instructionType = instructionToken.Type.ToInstruction();
        if (instructionType is null) throw new ArgumentOutOfRangeException();
        InstructionNode instructionNode = new(instructionType.Value);
        _instructions.Add(instructionNode);
    }

    private void ParseLd(Token instruction)
    {
        Token? operand1 = Peek();
        if (operand1 is null) { Error(instruction); return; }
        switch (operand1.Type)
        {
            case TokenType.Bc: case TokenType.De: case TokenType.Hl: case TokenType.Sp:
            {
                Consume();
                Token? token = Peek();
                if (token is null) { Error(instruction); return; }

                RegisterType? registerType = operand1.Type.ToRegisterType();
                if (registerType is null) throw new ArgumentOutOfRangeException();
                OperandNode operandNode1 = new(registerType.Value);
                OperandNode operandNode2 = token.Type is TokenType.LBracket ? ParseAddressAccess() : ParseNumber();
                
                
                _instructions.Add(new InstructionNode(InstructionType.Ld, operandNode1, operandNode2));
                
                return;
            }
        }//TODO finish
    }

    private OperandNode ParseAddressAccess()
    {
        Consume();
        throw new NotImplementedException();
    }

    private OperandNode ParseNumber()
    {
        throw new NotImplementedException();
    }
}