namespace Z80Assembler.Ast;

public class InstructionNode
{
    public InstructionType Type { get; }
    public OperandNode? Operand1 { get; }
    public OperandNode? Operand2 { get; }

    public InstructionNode(InstructionType type, OperandNode? operand1, OperandNode? operand2)
    {
        Type = type;
        Operand1 = operand1;
        Operand2 = operand2;
    }
}