using Z80CCompiler.Parsing.ASTNodes.Factor;

namespace Z80CCompiler.Parsing.ASTNodes;

public class IntFactor : IFactor
{
    public int Integer { get; }

    public IntFactor(int integer)
    {
        Integer = integer;
    }
}