namespace Z80Assembler;

public class LabelLocation
{
    public string Label { get; }
    public int CodePosition { get; }

    public LabelLocation(string label, int codePosition)
    {
        Label = label;
        CodePosition = codePosition;
    }
}