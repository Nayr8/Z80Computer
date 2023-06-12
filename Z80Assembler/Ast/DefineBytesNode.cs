
namespace Z80Assembler.Ast;

public class DefineBytesNode : INode
{
    // Exactly 1 of the int or string is in each entry
    public List<(int?, string?)> Bytes { get; }

    public DefineBytesNode(List<(int?, string?)> bytes)
    {
        Bytes = bytes;
    }
}