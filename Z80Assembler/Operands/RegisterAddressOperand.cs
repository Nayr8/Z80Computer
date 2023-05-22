namespace Z80Assembler.Operands
{
    public class RegisterAddressOperand : IOperand
    {
        public Register Register { get; set; }

        public RegisterAddressOperand(Register register)
        {
            Register = register;
        }
    }
}
