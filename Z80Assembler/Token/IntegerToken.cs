namespace Z80Assembler.Token;

public class IntegerToken : IToken
{
    public int Integer { get; }

    public IntegerToken(int integer, int line, int column) : base(line, column)
    {
        Integer = integer;
    }

    public override string ToString()
    {
        return Integer.ToString();
    }
}