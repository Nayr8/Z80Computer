namespace Z80Assembler.Token;

public class CommaToken : IToken
{
    public CommaToken(int line, int column) : base(line, column) {}

    public override string ToString()
    {
        return ",";
    }
}