namespace Z80Assembler.Token;

public class CommaToken : IToken
{
    public CommaToken(int line) : base(line) {}

    public override string ToString()
    {
        return ",";
    }
}