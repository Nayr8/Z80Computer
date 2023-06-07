namespace Z80Assembler.Tokens;

public class Token
{
    public TokenType Type { get; set; }
    public int Line { get; set; }
    public string? StringValue { get; set; }
    public int Integer { get; set; }
}