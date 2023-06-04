namespace Z80CCompiler.Parsing.Tokens;

public class IntegerLiteralToken : IToken
{
    public int Integer { get; }

    public IntegerLiteralToken(int integer)
    {
        Integer = integer;
    }

    public override string ToString()
    {
        return $"{Integer}";
    }
}