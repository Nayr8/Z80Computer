namespace Z80Assembler.Ast;

public class DefineWordsNode
{
    public List<int> Words { get; }

    public DefineWordsNode(List<int> words)
    {
        Words = words;
    }
}