namespace Z80Assembler.Operands
{
    public class LiteralOperand : IOperand
    {
        public int Value { get; set; }
        public LiteralOperand(int value)
        {
            Value = value;
        }
    }
}
