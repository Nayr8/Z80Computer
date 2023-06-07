using Z80Assembler.old.Token;

namespace Z80Assembler.old.Error;

public class SyntaxError
{
    public int Line { get; }
    public string? BadSyntax { get; } // Null means buffer overrun
    public IToken? BadToken { get; }

    public SyntaxError(int line)
    {
        Line = line;
        BadSyntax = null;
    }
    
    public SyntaxError(int line, string badSyntax)
    {
        if (badSyntax == "\n")
        {
            throw new Exception();
        }
        Line = line;
        BadSyntax = badSyntax;
    }
    
    public SyntaxError(IToken badToken)
    {
        Line = badToken.Line;
        BadToken = badToken;
    }

    public override string ToString()
    {
        return $"({Line}): Invalid Syntax: {BadSyntax}";
    }
}