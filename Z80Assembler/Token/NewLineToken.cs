namespace Z80Assembler.Token;

public class NewLineToken : IToken
{
    public NewLineToken(int line, int column) : base(line, column) {}

    public override string ToString()
    {
        return "\\n\n";
    }
}