namespace Z80CCompiler.Parsing.ASTNodes.Expression;
public class BitwiseOrExpression
{
    public BitwiseXorExpression BitwiseXorExpression { get; }
    public List<BitwiseXorExpression> BitwiseOrBitwiseXorExpressions { get; } = new();

    public BitwiseOrExpression(BitwiseXorExpression bitwiseXorExpression)
    {
        BitwiseXorExpression = bitwiseXorExpression;
    }
}
