namespace Z80CCompiler.Parsing.ASTNodes;

public class IntFactor
{
    public int Integer { get; }

    public IntFactor(int integer)
    {
        Integer = integer;
    }
}