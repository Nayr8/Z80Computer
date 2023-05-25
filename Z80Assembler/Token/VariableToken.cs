namespace Z80Assembler.Token;

public class VariableToken : IToken
{
    public string Label { get; }

    public VariableToken(string label, int line, int column) : base(line, column)
    {
        Label = label;
    }

    public override string ToString()
    {
        return $"@{Label}";
    }
}