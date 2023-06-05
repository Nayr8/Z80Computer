using Z80CCompiler.Parsing.ASTNodes.Expression;

namespace Z80CCompiler.Parsing.ASTNodes;

public class LogicalAndExpression
{
    public BitwiseOrExpression BitwiseOrExpression { get; }
    public List<BitwiseOrExpression> BitwiseOrAndExpressions = new();

    public LogicalAndExpression(BitwiseOrExpression bitwiseOrExpression)
    {
        BitwiseOrExpression = bitwiseOrExpression;
    }
}