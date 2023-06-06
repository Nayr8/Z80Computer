
using System.Text;

namespace Z80Linker;
public class ObjectFile
{
    private byte[] _data;
    private int _cursor;
    private List<SectionHeader> _headers = new();
    private Dictionary<string, LabelLocation> _labels = new();
    private Dictionary<string, LabelLocation> _labelAbsoluteDestination = new();
    private Dictionary<string, LabelLocation> _labelRelativeDestination = new();

    public ObjectFile(byte[] data)
    {
        _data = data;
        Parse();
    }

    public ArraySegment<byte>? GetSection(string section)
    {
        SectionHeader? header = _headers.Find(header => header.SectionName == section);
        if (header is null) return null;
        return new ArraySegment<byte>(_data, header.SectionOffset, header.SectionLength);
    }

    public bool SectionExists(string section)
    {
        return _headers.Find(header => header.SectionName == section) != null;
    }

    private void Parse()
    {
        int magicNumber = NextInt();
        int headerCount = NextWord();
        int labelCount = NextWord();
        int absoluteLabelDestCount = NextWord();
        int relativeLabelDestCount = NextWord();

        for (int i = 0; i < headerCount; ++i)
        {
            SectionHeader sectionHeader = ParseHeader();
            _headers.Add(sectionHeader);
        }
        for (int i = 0; i < labelCount; ++i)
        {
            (string labelName, LabelLocation labelLocation) = ParseLabelLocation();
            _labels.Add(labelName, labelLocation);
        }
        for (int i = 0; i < absoluteLabelDestCount; ++i)
        {
            (string labelName, LabelLocation labelLocation) = ParseLabelLocation();
            _labelAbsoluteDestination.Add(labelName, labelLocation);
        }
        for (int i = 0; i < relativeLabelDestCount; ++i)
        {
            (string labelName, LabelLocation labelLocation) = ParseLabelLocation();
            _labelRelativeDestination.Add(labelName, labelLocation);
        }
    }

    private SectionHeader ParseHeader()
    {
        int sectionOffset = NextWord();
        int sectionSize = NextWord();

        int nameOffset = NextWord();
        int nameLength = NextByte();
        string sectionName = GetString(nameOffset, nameLength);
        return new SectionHeader(sectionOffset, sectionSize, sectionName);
    }

    private (string, LabelLocation) ParseLabelLocation()
    {
        int labelNameOffset = NextWord();
        int labelNameLength = NextByte();
        string labelName = GetString(labelNameOffset, labelNameLength);
        int codeSectionNameOffset = NextWord();
        int codeSectionNameLength = NextByte();
        string codeSectionName = GetString(codeSectionNameOffset, codeSectionNameLength);
        int location = NextWord();
        return (labelName, new LabelLocation(location, codeSectionName));
    }

    private string GetString(int offset, int length)
    {
        StringBuilder sb = new();
        for (int i = 0; i < length; ++i)
        {
            sb.Append((char)_data[offset + i]);
        }
        return sb.ToString();
    }

    private int NextByte()
    {
        return _data[_cursor++];
    }

    private int NextWord()
    {
        return NextByte() | (NextByte() >> 8);
    }

    private int NextInt()
    {
        return NextWord() | (NextWord() >> 16);
    }
}
