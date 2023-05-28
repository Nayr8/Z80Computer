namespace Z80Assembler.Token;

public class TextToken : IToken
{
    public string Text { get; }

    public TextToken(string text, int line) : base(line)
    {
        Text = text;
    }
    
    public override string ToString()
    {
        return Text;
    }
}