namespace Z80Assembler.Operands
{
    public class RegisterOperand : IOperand
    {
        public Register Register { get; set; }

        public RegisterOperand(Register register)
        {
            Register = register;
        }
    }
}
