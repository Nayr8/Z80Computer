namespace Z80CCompiler.Parsing.ASTNodes.Factor;

public class Factor
{
    public IFactor Inner { get; }

    public Factor(IFactor factor)
    {
        Inner = factor;
    }
}