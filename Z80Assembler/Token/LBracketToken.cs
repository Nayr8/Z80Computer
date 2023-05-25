namespace Z80Assembler.Token;

public class LBracketToken : IToken
{
    public LBracketToken(int line, int column) : base(line, column) {}

    public override string ToString()
    {
        return "(";
    }
}