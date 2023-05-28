namespace Z80Assembler.Token;

public class NewLineToken : IToken
{
    public NewLineToken(int line) : base(line) {}

    public override string ToString()
    {
        return "\\n\n";
    }
}