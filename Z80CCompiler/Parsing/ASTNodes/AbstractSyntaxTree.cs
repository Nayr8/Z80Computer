namespace Z80CCompiler.Parsing.ASTNodes;

public class AbstractSyntaxTree
{
    public Function Function { get; }

    public AbstractSyntaxTree(Function function)
    {
        Function = function;
    }
}