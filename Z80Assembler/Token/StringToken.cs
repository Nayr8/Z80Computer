namespace Z80Assembler.Token;

public class StringToken : IToken
{
    public string Value { get; }
    
    public StringToken(string value, int line) : base(line)
    {
        Value = value;
    }
    
    public override string ToString()
    {
        return $"\"{Value}\"";
    }
}