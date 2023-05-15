

namespace Z80CPUEmulator
{
    public class Flags
    {
        public byte Value { get; set; }

        public bool Carry
        {
            get => MathHelper.GetBit(Value, 0);
            set => Value = MathHelper.SetBit(Value, value, 0);
        }
        public bool AddSubtract
        {
            get => MathHelper.GetBit(Value, 1);
            set => Value = MathHelper.SetBit(Value, value, 1);
        }
        public bool Overflow
        {
            get => MathHelper.GetBit(Value, 2);
            set => Value = MathHelper.SetBit(Value, value, 2);
        }
        public bool Parity
        {
            get => Overflow;
            set => Overflow = value;
        }
        public bool HalfCarry
        {
            get => MathHelper.GetBit(Value, 4);
            set => Value = MathHelper.SetBit(Value, value, 4);
        }
        public bool Zero
        {
            get => MathHelper.GetBit(Value, 6);
            set => Value = MathHelper.SetBit(Value, value, 6);
        }
        public bool Sign
        {
            get => MathHelper.GetBit(Value, 7);
            set => Value = MathHelper.SetBit(Value, value, 7);
        }
    }
}
