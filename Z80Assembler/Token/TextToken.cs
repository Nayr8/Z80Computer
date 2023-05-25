namespace Z80Assembler.Token;

public class TextToken : IToken
{
    public string Text { get; }

    public TextToken(string text, int line, int column) : base(line, column)
    {
        Text = text;
    }
    
    public override string ToString()
    {
        return Text;
    }
}