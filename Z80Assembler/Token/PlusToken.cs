namespace Z80Assembler.Token;

public class PlusToken : IToken
{
    public PlusToken(int line, int column) : base(line, column) {}

    public override string ToString()
    {
        return "+";
    }
}