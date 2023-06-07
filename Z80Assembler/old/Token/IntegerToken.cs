namespace Z80Assembler.old.Token;

public class IntegerToken : IToken
{
    public int Integer { get; }

    public IntegerToken(int integer, int line) : base(line)
    {
        Integer = integer;
    }

    public override string ToString()
    {
        return Integer.ToString();
    }
}