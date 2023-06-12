namespace Z80Assembler.Ast;

public class DefineWordsNode : INode
{
    public List<int> Words { get; }

    public DefineWordsNode(List<int> words)
    {
        Words = words;
    }
}