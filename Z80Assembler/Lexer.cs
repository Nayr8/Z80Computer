using System.Text;
using Z80Assembler.Tokens;

namespace Z80Assembler;

public class Lexer
{
    private const char EndOfFile = (char)5;
    private string _code;
    private int _cursor;
    private int _line;
    private List<Token> _tokens = new();


    private char Peek() => _cursor < _code.Length ? _code[_cursor] : EndOfFile;
    private void Consume() => ++_cursor;
    private char Next() => _cursor < _code.Length ? _code[_cursor++] : EndOfFile;
    private void NewLine() => ++_line;

    public static List<Token> Run(string code)
    {
        return new Lexer(code).Tokenize();
    }

    private Lexer(string code)
    {
        _code = code.ReplaceLineEndings("\n");
    }

    private void AddToken(TokenType tokenType)
    {
        Token token = new Token()
        {
            Type = tokenType,
            Line = _line
        };
        _tokens.Add(token);
    }

    private void AddLabelToken(string value)
    {
        Token token = new Token()
        {
            Type = TokenType.Label,
            Line = _line,
            StringValue = value
        };
        _tokens.Add(token);
    }

    private void AddStringToken(string value)
    {
        Token token = new Token()
        {
            Type = TokenType.String,
            Line = _line,
            StringValue = value
        };
        _tokens.Add(token);
    }

    private void AddIdentifierToken(string identifier)
    {
        Token token = new Token()
        {
            Type = TokenType.Identifier,
            Line = _line,
            StringValue = identifier
        };
        _tokens.Add(token);
    }

    private void AddIntegerToken(int integer)
    {
        Token token = new Token()
        {
            Type = TokenType.Integer,
            Line = _line,
            Integer = integer
        };
        _tokens.Add(token);
    }

    private List<Token> Tokenize()
    {
        while (Next() is var next and not EndOfFile)
        {
            switch (next)
            {
                case ',': AddToken(TokenType.Comma); break;
                case '(': AddToken(TokenType.LBracket); break;
                case ')': AddToken(TokenType.RBracket); break;
                case '+': AddToken(TokenType.Plus); break;
                case '\n': AddToken(TokenType.LineEnd); NewLine(); break;
                case '"': StringToken(); break;
                case ' ': break;
                case >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_' or '.': IdentifierOrKeywordToken(next); break;
                case >= '0' and <= '9': IntegerToken(next); break;
                default: AddToken(TokenType.Invalid); break;
            }
        }

        return _tokens;
    }

    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        {"section", TokenType.Section},
        {"nop", TokenType.Nop},
        {"ld", TokenType.Ld},
        {"inc", TokenType.Inc},
        {"dec", TokenType.Dec},
        {"rlca", TokenType.Rlca},
        {"ex", TokenType.Ex},
        {"add", TokenType.Add},
        {"rrca", TokenType.Rrca},
        {"djnz", TokenType.Djnz},
        {"rla", TokenType.Rla},
        {"rra", TokenType.Rra},
        {"jr", TokenType.Jr},
        {"daa", TokenType.Daa},
        {"cpl", TokenType.Cpl},
        {"scf", TokenType.Scf},
        {"ccf", TokenType.Ccf},
        {"halt", TokenType.Halt},
        {"adc", TokenType.Adc},
        {"sub", TokenType.Sub},
        {"sbc", TokenType.Sbc},
        {"and", TokenType.And},
        {"xor", TokenType.Xor},
        {"or", TokenType.Or},
        {"cp", TokenType.Cp},
        {"ret", TokenType.Ret},
        {"pop", TokenType.Pop},
        {"jp", TokenType.Jp},
        {"call", TokenType.Call},
        {"push", TokenType.Push},
        {"rst", TokenType.Rst},
        {"exx", TokenType.Exx},
        {"in", TokenType.In},
        {"di", TokenType.Di},
        {"ei", TokenType.Ei},
        {"rlc", TokenType.Rlc},
        {"rrc", TokenType.Rrc},
        {"rl", TokenType.Rl},
        {"rr", TokenType.Rr},
        {"sla", TokenType.Sla},
        {"sra", TokenType.Sra},
        {"sll", TokenType.Sll},
        {"srl", TokenType.Srl},
        {"bit", TokenType.Bit},
        {"res", TokenType.Res},
        {"set", TokenType.Set},
        {"neg", TokenType.Neg},
        {"retn", TokenType.Retn},
        {"im", TokenType.Im},
        {"reti", TokenType.Reti},
        {"rrd", TokenType.Rrd},
        {"rld", TokenType.Rld},
        {"ldi", TokenType.Ldi},
        {"cpi", TokenType.Cpi},
        {"ini", TokenType.Ini},
        {"outi", TokenType.Outi},
        {"ldd", TokenType.Ldd},
        {"cpd", TokenType.Cpd},
        {"ind", TokenType.Ind},
        {"outd", TokenType.Outd},
        {"ldir", TokenType.Ldir},
        {"cpir", TokenType.Cpir},
        {"inir", TokenType.Inir},
        {"otir", TokenType.Otir},
        {"lddr", TokenType.Lddr},
        {"cpdr", TokenType.Cpdr},
        {"indr", TokenType.Indr},
        {"otdr", TokenType.Otdr},
        {"db", TokenType.Db},
        {"dw", TokenType.Dw},
        {"a", TokenType.A},
        {"b", TokenType.B},
        {"c", TokenType.C},
        {"d", TokenType.D},
        {"e", TokenType.E},
        {"h", TokenType.H},
        {"l", TokenType.L},
        {"af", TokenType.Af},
        {"af'", TokenType.AfShadow},
        {"bc", TokenType.Bc},
        {"de", TokenType.De},
        {"hl", TokenType.Hl},
        {"ix", TokenType.Ix},
        {"iy", TokenType.Iy},
        {"ixh", TokenType.Ixh},
        {"ixl", TokenType.Ixl},
        {"iyh", TokenType.Iyh},
        {"iyl", TokenType.Iyl},
        {"sp", TokenType.Sp},
    };

    private void IdentifierOrKeywordToken(char first)
    {
        StringBuilder sb = new();
        sb.Append(first);
        
        while (Peek() is var next and (>= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '\'' or '.'))
        {
            Consume();
            sb.Append(next);
        }

        string value = sb.ToString();
        if (Peek() is ':')
        {
            Consume();
            AddLabelToken(value);
        }
        else if (Keywords.TryGetValue(value, out TokenType keyword))
        {
            AddToken(keyword);
        }
        else
        {
            AddIdentifierToken(value);
        }
    }

    private void IntegerToken(char first)
    {
        int value = 0;
        if (first is '0')
        {
            switch (Peek())
            {
                case 'x':
                    bool done = false;
                    while (!done)
                    {
                        char next = Peek();
                        switch (next)
                        {
                            case >= '0' and <= '9':
                                Consume();
                                value *= 16;
                                value += next - '0';
                                break;
                            case >= 'A' and <= 'F':
                                Consume();
                                value *= 16;
                                value += next - 'A' + 10;
                                break;
                            case >= 'a' and <= 'f':
                                Consume();
                                value *= 16;
                                value += next - 'a' + 10;
                                break;
                            default:
                                done = true;
                                break;
                        }
                    }

                    while (Peek() is var next and (>= '0' and <= '9' or >= 'A' and <= 'Z' or >= 'a' and <= 'z'))
                    {
                        Consume();
                        value *= 2;
                        value += next - '0';
                    }

                    AddIntegerToken(value);
                    return;
                case 'b':
                    while (Peek() is var next and >= '0' and <= '1')
                    {
                        Consume();
                        value *= 2;
                        value += next - '0';
                    }

                    AddIntegerToken(value);
                    return;
            }
        }

        value = first - '0';
        while (Peek() is var next and >= '0' and <= '9')
        {
            Consume();
            value *= 10;
            value += next - '0';
        }

        AddIntegerToken(value);
    }

    private void StringToken()
    {
        StringBuilder sb = new();
        while (Peek() is var next and not '\n' and not EndOfFile and not '"')
        {
            Consume();
            sb.Append(next);
        }

        if (Peek() is not '"')
        {
            AddToken(TokenType.Invalid);
            return;
        }
        AddStringToken(sb.ToString());
    }
}