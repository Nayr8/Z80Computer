namespace Z80CCompiler.Assembling;

public class AssembledCode
{
    public List<byte> Code { get; } = new();
    public Dictionary<string, int> Labels { get; } = new ();

    public void AddLabel(string label)
    {
        Labels.Add(label, Code.Count);
    }

    public void AddByte(byte value)
    {
        Code.Add(value);
    }

    public void AddWord(ushort value)
    {
        Code.Add((byte)value);
        Code.Add((byte)(value >> 8));
    }
}