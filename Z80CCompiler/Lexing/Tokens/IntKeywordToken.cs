namespace Z80CCompiler.Parsing.Tokens;

public class IntKeywordToken : IToken
{
    public override string ToString()
    {
        return "int";
    }
}