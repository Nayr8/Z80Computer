namespace Z80CCompiler.Parsing.ASTNodes;

public class Term
{
    public object Factor { get; }
    public List<(MulDivOp, object)> MulDivFactors = new();

    public Term(object factor)
    {
        Factor = factor;
    }
}