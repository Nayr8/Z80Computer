namespace Z80Assembler.Token;

public class RBracketToken : IToken
{
    public RBracketToken(int line) : base(line) {}

    public override string ToString()
    {
        return ")";
    }
}