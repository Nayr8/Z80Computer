namespace Z80Assembler.Token;

public class BrokenToken : IToken
{
    public override string ToString()
    {
        return "#";
    }
}