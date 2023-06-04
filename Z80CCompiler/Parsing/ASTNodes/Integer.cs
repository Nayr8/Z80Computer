namespace Z80CCompiler.Parsing.ASTNodes;

public class Integer
{
    public int Value { get; }

    public Integer(int value)
    {
        Value = value;
    }
}