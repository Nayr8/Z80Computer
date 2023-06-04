namespace Z80CCompiler.Parsing.ASTNodes;

public class RelationalExpression
{
    public AdditiveExpression AdditiveExpression { get; }
    public List<(RelationalOp, AdditiveExpression)> RelationalAdditiveExpressions = new();

    public RelationalExpression(AdditiveExpression additiveExpression)
    {
        AdditiveExpression = additiveExpression;
    }
}