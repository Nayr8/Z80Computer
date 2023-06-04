using Z80CCompiler.Parsing.ASTNodes.Factor;

namespace Z80CCompiler.Parsing.ASTNodes;

public class Term
{
    public object Factor { get; }
    public List<(MulDivOp, IFactor)> MulDivFactors = new();

    public Term(IFactor factor)
    {
        Factor = factor;
    }
}