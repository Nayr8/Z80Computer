namespace Z80Assembler.Token;

public class LabelToken : IToken
{
    public string Label { get; }

    public LabelToken(string label)
    {
        Label = label;
    }

    public override string ToString()
    {
        return $"{Label}:";
    }
}