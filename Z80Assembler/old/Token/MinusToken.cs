namespace Z80Assembler.old.Token;

public class MinusToken : IToken
{
    public MinusToken(int line) : base(line) {}

    public override string ToString()
    {
        return "-";
    }
}