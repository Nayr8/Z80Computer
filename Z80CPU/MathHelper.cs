

namespace Z80CPUEmulator
{
    public static class MathHelper
    {
        public static bool GetBit(byte value, int offset)
        {
            return (value & (1 << offset)) != 0;
        }

        public static byte SetBit(byte value, bool bit, int offset)
        {
            if (bit)
            {
                value |= (byte)(1 << offset);
            }
            else
            {
                value &= Invert((byte)(1 << offset));
            }
            return value;
        }

        public static byte Invert(byte value)
        {
            return (byte)~value;
        }
    }
}
