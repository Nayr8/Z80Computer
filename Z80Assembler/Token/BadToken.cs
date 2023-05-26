namespace Z80Assembler.Token;

public class BadToken : IToken
{
    public BadToken(int line, int column) : base(line, column) {}

    public override string ToString()
    {
        return "#";
    }
}