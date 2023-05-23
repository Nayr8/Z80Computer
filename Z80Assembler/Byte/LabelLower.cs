namespace Z80Assembler;

public class LabelLower : IByte
{
    public string Label { get; set; }

    public LabelLower(string label)
    {
        Label = label;
    }
}