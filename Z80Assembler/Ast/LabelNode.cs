namespace Z80Assembler.Ast;
public class LabelNode : INode
{
    public string Label { get; }

    public LabelNode(string label)
    {
        Label = label;
    }
}
