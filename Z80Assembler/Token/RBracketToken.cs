namespace Z80Assembler.Token;

public class RBracketToken : IToken
{
    public RBracketToken(int line, int column) : base(line, column) {}

    public override string ToString()
    {
        return ")";
    }
}