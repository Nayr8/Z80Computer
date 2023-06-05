using System.Security.Cryptography;

namespace Z80CCompiler.Assembling;

public class GeneratedCode
{
    public List<byte> Code { get; } = new();
    public Dictionary<string, int> Labels { get; } = new ();
    private Dictionary<string, int> _tempLabels = new ();
    private List<(string, int)> _tempLabelPointer = new(); // Only absolute
    private int tempLabelNumber = 0;

    private const string tempLabelPre = "_temporyLabelfhdivhenod";

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

    public string GenerateTempLabel()
    {
        string label = tempLabelPre + tempLabelNumber;
        ++tempLabelNumber;
        return label;
    }

    public void AddTempLabel(string label)
    {
        _tempLabels.Add(label, Code.Count);
    }

    public void AddTempLabelPointer(string label)
    {
        _tempLabelPointer.Add((label, Code.Count));
        AddWord(0x0000); // Temp val
    }

    public void ResolveTempLabels()
    {
        foreach ((string label, int source) in _tempLabelPointer)
        {
            if (_tempLabels.TryGetValue(label, out int labelPosition))
            {
                Code[source] = (byte)labelPosition;
                Code[source + 1] = (byte)(labelPosition >> 8);
            } else
            {
                throw new Exception(); // TODO better errors
            }
        }
        _tempLabelPointer.Clear(); // Prevent reuse
    }
}