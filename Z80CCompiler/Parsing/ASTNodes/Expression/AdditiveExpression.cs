namespace Z80CCompiler.Parsing.ASTNodes;

public class AdditiveExpression
{
    public Term Term { get; }
    public List<(AddSubOp, Term)> AddSubTerms = new();

    public AdditiveExpression(Term term)
    {
        Term = term;
    }
}