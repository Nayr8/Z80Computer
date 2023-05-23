namespace Z80Assembler;

public class LabelUpper : IByte
{
    public string Label { get; set; }

    public LabelUpper(string label)
    {
        Label = label;
    }
}