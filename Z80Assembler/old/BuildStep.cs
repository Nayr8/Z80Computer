namespace Z80Assembler.old;

public class BuildStep
{
    public string File { get; }
    public int? AddressAlign { get; }

    public BuildStep(string file, int? addressAlign)
    {
        File = file;
        AddressAlign = addressAlign;
    }
}