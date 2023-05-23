namespace Z80Assembler;

public class Byte
{
    public string? Label { get; set; }
    public IByte Value { get; set; }

    public Byte(IByte value)
    {
        Value = value;
    }

    public static Byte[] Bytes(byte v0)
    {
        return new[] { new Byte(new RawByte(v0)) };
    }

    public static Byte[] Bytes(byte v0, byte v1)
    {
        return new[] { new Byte(new RawByte(v0)), new Byte(new RawByte(v1)) };
    }

    public static Byte[] Bytes(byte v0, byte v1, byte v2)
    {
        return new[] { new Byte(new RawByte(v0)), new Byte(new RawByte(v1)), new Byte(new RawByte(v2)) };
    }

    public static Byte[] Bytes(byte v0, string label)
    {
        return new[] { new Byte(new RawByte(v0)), new Byte(new LabelLower(label)), new Byte(new LabelUpper(label)) };
    }
}