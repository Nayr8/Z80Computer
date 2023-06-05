using Z80CCompiler.Parsing.ASTNodes.Factor;

namespace Z80CCompiler.Parsing.ASTNodes.Expression;

public class LogicalOrExpression : IFactor
{
    public LogicalAndExpression LogicalAndExpression { get; }
    public List<LogicalAndExpression> OrEqualityExpressions = new();

    public LogicalOrExpression(LogicalAndExpression logicalAndExpression)
    {
        LogicalAndExpression = logicalAndExpression;
    }
}