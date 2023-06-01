namespace Z80Assembler.Token;

public class AbsoluteAlignToken : IToken
{
    public int AlignAddress { get; }
    public AbsoluteAlignToken(int alignAddress, int line) : base(line)
    {
        AlignAddress = alignAddress;
    }

    public override string ToString()
    {
        return $"Align(0x{AlignAddress:X4})";
    }
}