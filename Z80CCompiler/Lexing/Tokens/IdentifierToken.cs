namespace Z80CCompiler.Parsing.Tokens;

public class IdentifierToken : IToken
{
    public string Identifier { get; }

    public IdentifierToken(string identifier)
    {
        Identifier = identifier;
    }

    public override string ToString()
    {
        return $"'{Identifier}'";
    }
}