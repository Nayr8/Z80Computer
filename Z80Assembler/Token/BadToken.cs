namespace Z80Assembler.Token;

public class BadToken : IToken
{
    public BadToken(int line) : base(line) {}

    public override string ToString()
    {
        return "#";
    }
}