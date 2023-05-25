namespace Z80Assembler.Token;

public abstract class IToken
{
    public int Line { get; }
    public int Column { get; }

    protected IToken(int line, int column)
    {
        Line = line;
        Column = column;
    }
}