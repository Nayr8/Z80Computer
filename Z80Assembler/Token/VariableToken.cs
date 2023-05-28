namespace Z80Assembler.Token;

public class VariableToken : IToken
{
    public string Label { get; }

    public VariableToken(string label, int line) : base(line)
    {
        Label = label;
    }

    public override string ToString()
    {
        return $"@{Label}";
    }
}