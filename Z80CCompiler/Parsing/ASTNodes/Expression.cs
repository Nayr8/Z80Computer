namespace Z80CCompiler.Parsing.ASTNodes;

public class Expression
{
    public Term Term { get; }
    public List<(AddSubOp, Term)> AddSubTerms = new();

    public Expression(Term term)
    {
        Term = term;
    }
}