using Z80CCompiler.Parsing.ASTNodes.Expression;

namespace Z80CCompiler.Parsing.ASTNodes;

public class Statement
{
    public LogicalOrExpression Expression { get; }

    public Statement(LogicalOrExpression expression)
    {
        Expression = expression;
    }
}