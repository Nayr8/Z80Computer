namespace Z80CCompiler.Parsing.ASTNodes;

public class EqualityExpression
{
    public RelationalExpression RelationalExpression { get; }
    public List<(EqualityOp, RelationalExpression)> EqualityRelationalExpressions = new();

    public EqualityExpression(RelationalExpression relationalExpression)
    {
        RelationalExpression = relationalExpression;
    }
}