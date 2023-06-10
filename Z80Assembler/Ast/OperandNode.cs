namespace Z80Assembler.Ast;

public class OperandNode
{
    public bool Address;
    public OperandType OperandType;
    public RegisterType? RegisterType;
    public string? Label;
    public int? Integer;



    public OperandNode(RegisterType register, bool address)
    {
        OperandType = OperandType.Register;
    }

    public OperandNode(string label, bool address)
    {
        Address = address;
        OperandType = OperandType.Label;
    }

    public OperandNode(FlagCheckType flagCheck)
    {
        OperandType = OperandType.FlagCheck;
    }

    public OperandNode(int number, bool address)
    {
        Address = address;
        OperandType = OperandType.Register;
    }
}