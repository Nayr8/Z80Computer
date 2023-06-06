using System.Text;

namespace Z80Linker;

public class Section
{
    private string _name;
    private int _type;

    private List<Label> _labels = new();
    private List<Label> _externalLabels = new();
    private List<int> _absolutePositionsToBeOffset = new();

    private ArraySegment<byte> _code;

    public int NextSectionOffset { get; }

    public Section(byte[] file, int sectionOffset)
    {
        int sectionLength = file[sectionOffset] | (file[sectionOffset + 1] << 8);
        NextSectionOffset = sectionOffset + sectionLength;

        int nameOffset = file[sectionOffset + 2] | (file[sectionOffset + 3] << 8);
        _name = GetNullTerminatedString(file, nameOffset);
        
        _type = file[sectionOffset + 4];
        
        int codeOffset = file[sectionOffset + 5] | (file[sectionOffset + 6] << 8);
        int codeLength = file[sectionOffset + 7] | (file[sectionOffset + 8] << 8);
        _code = new ArraySegment<byte>(file, codeOffset, codeLength);

        int labelCount = file[sectionOffset + 9];
        ParseLabels(file, labelCount, sectionOffset + 10);

        int externalLabelStart = sectionOffset + 10 + labelCount * 4;
        int externalLabelCount = file[externalLabelStart];
        ParseExternalLabels(file, externalLabelCount, externalLabelStart + 1);

        int absolutePositionsToBeOffsetStart = externalLabelStart + 2 + externalLabelCount * 4;
        int absolutePositionsToBeOffsetCount = file[absolutePositionsToBeOffsetStart];
        ParseAbsolutePositionsToBeOffset(file, absolutePositionsToBeOffsetCount, absolutePositionsToBeOffsetStart + 1);
    }

    private void ParseLabels(IReadOnlyList<byte> file, int labelCount, int offset)
    {
        for (int i = offset; i < labelCount * 4; i += 4)
        {
            int nameOffset = file[i] | (file[i + 1] << 8);
            string name = GetNullTerminatedString(file, nameOffset);
            int location = file[i + 2] | (file[i + 3] << 8);
            _labels.Add(new Label(name, location));
        }
    }

    private void ParseExternalLabels(IReadOnlyList<byte> file, int externalLabelCount, int offset)
    {
        for (int i = offset; i < externalLabelCount * 4; i += 4)
        {
            int nameOffset = file[i] | (file[i + 1] << 8);
            string name = GetNullTerminatedString(file, nameOffset);
            int location = file[i + 2] | (file[i + 3] << 8);
            _externalLabels.Add(new Label(name, location));
        }
    }

    private void ParseAbsolutePositionsToBeOffset(IReadOnlyList<byte> file, int absolutePositionsToBeOffsetCount, int offset)
    {
        for (int i = offset; i < absolutePositionsToBeOffsetCount * 2; i += 2)
        {
            int localOffset = file[i] | (file[i + 1] << 8);
            _absolutePositionsToBeOffset.Add(localOffset);
        }
    }

    private static string GetNullTerminatedString(IReadOnlyList<byte> file, int offset)
    {
        StringBuilder sb = new();
        for (int i = offset; file[i] != 0; ++i)
        {
            sb.Append((char)file[i]);
        }
        return sb.ToString();
    }
}