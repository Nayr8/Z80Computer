namespace Z80Assembler.Exceptions;

public class IncorrectNumberOfOperandsException : AssemblerException
{
    public string Instruction { get; set; }
    public ISet<int> PossibleNumberOfOperands { get; set; }
    public int ActualNumberOfOperands { get; set; }

    public IncorrectNumberOfOperandsException(string instruction, int actualNumberOfOperands, ISet<int> possibleNumberOfOperands)
    {
        if (possibleNumberOfOperands.Contains(actualNumberOfOperands))
        {
            throw new InternalAssemblerException($"Valid number of operands '{actualNumberOfOperands}' found in 'IncorrectNumberOfOperandsException' for instruction '{instruction}'");
        }
        if (possibleNumberOfOperands.Count == 0)
        {
            throw new InternalAssemblerException(
                $"Instruction does not have any possible operand count for instruction '{instruction}'");
        }
        // TODO more validation

        Instruction = instruction;
        ActualNumberOfOperands = actualNumberOfOperands;
        PossibleNumberOfOperands = possibleNumberOfOperands;
    }
}