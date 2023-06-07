namespace Z80Assembler.old.Token;

public class LBracketToken : IToken
{
    public LBracketToken(int line) : base(line) {}

    public override string ToString()
    {
        return "(";
    }
}