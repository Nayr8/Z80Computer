using System.Globalization;
using System.Text;

namespace Z80Linker;

public class LinkerScript
{
    private const char EndOfFile = (char)5;
    private int _cursor;
    private char[] _file;
    public readonly List<(int, List<string>)> Mappings = new ();

    public LinkerScript(char[] file)
    {
        _file = file;
        Parse();
    }

    private void Parse()
    {
        while (Peek() is not EndOfFile)
        {
            Consume();
            Consume();
            int location = ParseHex();
            Consume();
            Consume();

            List<string> sectionNames = ParseSectionNames();
            
            Mappings.Add((location, sectionNames));
        }
    }

    private int ParseHex()
    {
        StringBuilder sb = new();
        while (Peek() is not EndOfFile and (>= '0' and <= '9' or >= 'A' and <= 'F') and var next)
        {
            sb.Append(next);
            Consume();
        }

        return int.Parse(sb.ToString(), NumberStyles.HexNumber);
    }

    private List<string> ParseSectionNames()
    {
        List<string> sectionNames = new();
        StringBuilder sb = new();
        
        Consume();
        while (Peek() is not ')')
        {
            while (Peek() is (>= 'a' and <= 'z'
                   or >= 'A' and <= 'Z' or >= '0' and <= '9'
                   or '.' or '_' or '-') and var next)
            {
                Consume();
                sb.Append(next);
            }
            

            if (sb.Length <= 0)
            {
                Consume();
                continue;
            }
            sectionNames.Add(sb.ToString());
            sb.Clear();
        }
        Consume();
        Consume();

        return sectionNames;
    }

    private char Peek() => _cursor < _file.Length ? _file[_cursor] : EndOfFile;
    private void Consume() => ++_cursor;
}