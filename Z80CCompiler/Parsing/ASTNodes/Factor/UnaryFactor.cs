using Z80CCompiler.Parsing.ASTNodes.Factor;

namespace Z80CCompiler.Parsing.ASTNodes;

public class UnaryFactor : IFactor
{
    public UnaryOp UnaryOp { get; }
    public object Factor { get; }

    public UnaryFactor(UnaryOp unaryOp, object factor)
    {
        UnaryOp = unaryOp;
        Factor = factor;
    }
}