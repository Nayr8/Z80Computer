namespace Z80Linker;
public class LabelLocation
{
    public int Offset { get; }
    public string Section { get; }

    public LabelLocation(int offset, string section)
    {
        Offset = offset;
        Section = section;
    }
}
