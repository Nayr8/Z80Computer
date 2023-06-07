namespace Z80Assembler.old.Token;

public class BadToken : IToken
{
    public BadToken(int line) : base(line) {}

    public override string ToString()
    {
        return "#";
    }
}