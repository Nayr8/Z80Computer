namespace Z80Assembler.Operands
{
    public class AddressOperand : IOperand
    {
        public int Address { get; set; }
        public AddressOperand(int address) {
            Address = address;
        }
    }
}
