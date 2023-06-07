namespace Z80Assembler.old.Token;

public class RBracketToken : IToken
{
    public RBracketToken(int line) : base(line) {}

    public override string ToString()
    {
        return ")";
    }
}