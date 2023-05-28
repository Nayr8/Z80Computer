namespace Z80Assembler.Token;

public abstract class IToken
{
    public int Line { get; }

    protected IToken(int line)
    {
        Line = line;
    }
}