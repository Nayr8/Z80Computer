using Z80CCompiler.Parsing.ASTNodes.Factor;

namespace Z80CCompiler.Parsing.ASTNodes;

public class Expression : IFactor
{
    public LogicalAndExpression LogicalAndExpression { get; }
    public List<LogicalAndExpression> OrEqualityExpressions = new();

    public Expression(LogicalAndExpression logicalAndExpression)
    {
        LogicalAndExpression = logicalAndExpression;
    }
}