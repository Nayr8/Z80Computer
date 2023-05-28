namespace Z80Assembler.Token;

public class PlusToken : IToken
{
    public PlusToken(int line) : base(line) {}

    public override string ToString()
    {
        return "+";
    }
}