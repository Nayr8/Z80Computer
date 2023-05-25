namespace Z80Assembler.Token;

public class BrokenToken : IToken
{
    public BrokenToken(int line, int column) : base(line, column) {}

    public override string ToString()
    {
        return "#";
    }
}