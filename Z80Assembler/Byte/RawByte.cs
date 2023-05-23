namespace Z80Assembler;

public class RawByte : IByte
{
    public byte Value { get; set; }

    public RawByte(byte value)
    {
        Value = value;
    }
}