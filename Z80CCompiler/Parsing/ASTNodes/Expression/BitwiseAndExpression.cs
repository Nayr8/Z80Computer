
namespace Z80CCompiler.Parsing.ASTNodes.Expression;
public class BitwiseAndExpression
{
    public EqualityExpression EqualityExpression { get; }
    public List<EqualityExpression> BitwiseAndEqualityExpressions { get; } = new();

    public BitwiseAndExpression(EqualityExpression equalityExpression)
    {
        EqualityExpression = equalityExpression;
    }
}
