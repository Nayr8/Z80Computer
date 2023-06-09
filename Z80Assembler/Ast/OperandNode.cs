namespace Z80Assembler.Ast;

public class OperandNode
{
    private bool _address;
    public OperandType OperandType;
    public RegisterType? RegisterType;
    public string? Label;



    public OperandNode(RegisterType register)
    {
        OperandType = OperandType.Register;
    }

    public OperandNode(string label, bool address)
    {
        _address = address;
        OperandType = OperandType.Label;
    }

    public OperandNode(FlagCheckType flagCheck)
    {
        OperandType = OperandType.FlagCheck;
    }

    public OperandNode(int number, bool address)
    {
        _address = address;
        OperandType = OperandType.Register;
    }
}