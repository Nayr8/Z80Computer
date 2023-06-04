namespace Z80CCompiler.Parsing.ASTNodes;

public class Function
{
    public string Identifier { get; }
    public Statement Statement { get; }

    public Function(string identifier, Statement statement)
    {
        Identifier = identifier;
        Statement = statement;
    }
}