namespace Z80CCompiler.Parsing.ASTNodes;

public class UnaryFactor
{
    public UnaryOp UnaryOp { get; }
    public object Factor { get; }

    public UnaryFactor(UnaryOp unaryOp, object factor)
    {
        UnaryOp = unaryOp;
        Factor = factor;
    }
}