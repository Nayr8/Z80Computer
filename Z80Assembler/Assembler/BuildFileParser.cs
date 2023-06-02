using System.Globalization;

namespace Z80Assembler;

public class BuildFileParser
{
    // TODO safety checking for the build file
    private const char EndOfFile = (char)0x05;
    private const char NewLine = '\n';
    private const char Space = ' ';
    
    private int _startAddress = -1;
    private bool _startAddressUsed = false;
    private readonly string _buildFile;
    private int _cursor;
    private string _projectDirectoryPath;
    
    private readonly StringBuffer _buffer = new(64);

    private List<BuildStep> _buildSteps = new();

    public BuildFileParser(string projectDirectoryPath)
    {
        _projectDirectoryPath = projectDirectoryPath;
        _buildFile = File.ReadAllText(Directory.GetFiles(projectDirectoryPath, "build.zbld")[0]);
    }

    private void Consume()
    {
        ++_cursor;
    }

    private char Peek()
    {
        return _cursor >= _buildFile.Length ? EndOfFile : _buildFile[_cursor];
    }

    public BuildStep[] Parse()
    {
        while (Peek() is not EndOfFile and var next)
        {
            switch (next)
            {
                case '0': NewStartAddress(); break;
                case Space or NewLine: Consume(); continue;
                default: NextFile(); break;
            }
            _buffer.Clear();
        }

        return _buildSteps.ToArray();
    }

    private void NewStartAddress()
    {
        Consume();
        Consume();

        while (Peek() is var next && IsHex(next))
        {
            _buffer.Add(next);
            Consume();
        }
        Consume();
        _startAddress = int.Parse(_buffer.ToString(), NumberStyles.HexNumber);
        _startAddressUsed = false;
    }

    private void NextFile()
    {
        while (Peek() is var next and not Space and not NewLine and not EndOfFile)
        {
            _buffer.Add(next);
            Consume();
        }

        AddBuildFolderOrFile(_buffer.ToString());
    }

    private void AddBuildFolderOrFile(string directoryOrFile)
    {
        try
        {
            string dir = Directory.GetDirectories(Path.Combine(_projectDirectoryPath, "src"), directoryOrFile)[0];
            foreach (string file in Directory.GetFiles(dir))
            {
                if (_startAddressUsed)
                {
                    _buildSteps.Add(new BuildStep(Path.Combine(directoryOrFile, Path.GetFileName(file)), null));
                }
                else
                {
                    _buildSteps.Add(new BuildStep(Path.Combine(directoryOrFile, Path.GetFileName(file)), _startAddress));
                    _startAddressUsed = true;
                }
            }
        } catch (Exception e)
        {
            if (_startAddressUsed)
            {
                _buildSteps.Add(new BuildStep(directoryOrFile, null));
            }
            else
            {
                _buildSteps.Add(new BuildStep(directoryOrFile, _startAddress));
                _startAddressUsed = true;
            }
        }
    }

    private static bool IsHex(char value)
    {
        return value is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
    }
}