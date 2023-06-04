namespace Z80CCompiler.Parsing.ASTNodes;

public class LogicalAndExpression
{
    public EqualityExpression EqualityExpression { get; }
    public List<EqualityExpression> AndEqualityExpressions = new();

    public LogicalAndExpression(EqualityExpression equalityExpression)
    {
        EqualityExpression = equalityExpression;
    }
}