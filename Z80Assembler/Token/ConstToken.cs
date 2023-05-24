namespace Z80Assembler.Token;

public class ConstToken : IToken
{
    public string Label { get; }

    public ConstToken(string label)
    {
        Label = label;
    }

    public override string ToString()
    {
        return $"@{Label}";
    }
}