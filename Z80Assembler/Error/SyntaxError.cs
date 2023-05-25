using Z80Assembler.Token;

namespace Z80Assembler.Error;

public class SyntaxError
{
    public int Line { get; }
    public int Column { get; }
    public string? BadSyntax { get; } // Null means buffer overrun
    public IToken? BadToken { get; }

    public SyntaxError(int line, int column)
    {
        Line = line;
        Column = column;
        BadSyntax = null;
    }
    
    public SyntaxError(int line, int column, string badSyntax)
    {
        if (badSyntax == "\n")
        {
            throw new Exception();
        }
        Line = line;
        Column = column;
        BadSyntax = badSyntax;
    }
    
    public SyntaxError(IToken badToken)
    {
        Line = badToken.Line;
        Column = badToken.Column;
        BadToken = badToken;
    }

    public override string ToString()
    {
        return $"({Line}, {Column}): Invalid Syntax: {BadSyntax}";
    }
}