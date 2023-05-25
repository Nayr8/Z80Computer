namespace Z80Assembler.Token;

public class LabelToken : IToken
{
    public string Label { get; }

    public LabelToken(string label, int line, int column) : base(line, column)
    {
        Label = label;
    }

    public override string ToString()
    {
        return $"{Label}:";
    }
}