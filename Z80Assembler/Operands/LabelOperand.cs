namespace Z80Assembler.Operands
{
    public class LabelOperand : IOperand
    {
        public string Label { get; set; }

        public LabelOperand(string label)
        {
            Label = label;
        }
    }
}
