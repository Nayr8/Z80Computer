namespace Z80Assembler.Token;

public class LBracketToken : IToken
{
    public LBracketToken(int line) : base(line) {}

    public override string ToString()
    {
        return "(";
    }
}