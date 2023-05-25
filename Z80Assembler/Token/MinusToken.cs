namespace Z80Assembler.Token;

public class MinusToken : IToken
{
    public MinusToken(int line, int column) : base(line, column) {}

    public override string ToString()
    {
        return "-";
    }
}