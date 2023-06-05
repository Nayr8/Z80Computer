namespace Z80CCompiler.Parsing.ASTNodes.Expression;
public class BitwiseXorExpression
{
    public BitwiseAndExpression BitwiseAndExpression { get; }
    public List<BitwiseAndExpression> BitwiseXorBitwiseAndExpressions { get; } = new();

    public BitwiseXorExpression(BitwiseAndExpression bitwiseAndExpression)
    {
        BitwiseAndExpression = bitwiseAndExpression;
    }
}
