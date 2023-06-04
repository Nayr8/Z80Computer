namespace Z80CCompiler.Parsing.ASTNodes;

public class Statement
{
    public Expression Expression { get; }

    public Statement(Expression expression)
    {
        Expression = expression;
    }
}