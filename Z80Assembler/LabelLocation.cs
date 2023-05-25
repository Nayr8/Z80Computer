namespace Z80Assembler;

public class LabelLocation
{
    public string Label { get; }
    public int Line { get; }
    public int Column { get; }

    public LabelLocation(string label, int line, int column)
    {
        Label = label;
        Line = line;
        Column = column;
    }
}