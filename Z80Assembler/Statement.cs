using Z80Assembler.Operands;

namespace Z80Assembler
{
    public class Statement
    {
        public string? Label;
        public string Instruction;
        public IOperand[] Operands;

        public Statement(string? label, string instruction, IOperand[] operands)
        {
            Label = label;
            Instruction = instruction;
            Operands = operands;
        }
    }
}
