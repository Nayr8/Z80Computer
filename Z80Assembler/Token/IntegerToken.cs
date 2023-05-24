namespace Z80Assembler.Token;

public class IntegerToken : IToken
{
    public int Integer { get; }

    public IntegerToken(int integer)
    {
        Integer = integer;
    }

    public override string ToString()
    {
        return Integer.ToString();
    }
}