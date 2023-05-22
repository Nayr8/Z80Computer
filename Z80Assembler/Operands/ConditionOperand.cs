namespace Z80Assembler.Operands
{
    public class ConditionOperand : IOperand
    {
        public Condition Condition { get; set; }
        public ConditionOperand(Condition condition)
        {
            Condition = condition;
        }
    }
}
