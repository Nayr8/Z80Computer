namespace Z80CCompiler.Parsing.Tokens;

public class ReturnKeywordToken : IToken
{
    public override string ToString()
    {
        return "return";
    }
}