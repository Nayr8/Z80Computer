namespace Z80Linker;

public class Linker
{
    private List<ObjectFile> _objectFiles = new();
    private List<byte> _code = new();
    
    private Dictionary<string, int> _labels = new();
    private List<Label> _externalLabels = new();

    public void Assemble(LinkerScript linkerScript)
    {
        foreach ((int start, List<string> sections) in linkerScript.Mappings)
        {
            while (_code.Count < start) _code.Add(0x00);
            foreach (string sectionName in sections)
            {
                AssembleSection(sectionName);
            }
        }
        ResolveExternalLabels();
    }

    private void AssembleSection(string sectionName)
    {
        foreach (ObjectFile objectFile in _objectFiles)
        {
            if (objectFile.Sections.TryGetValue(sectionName, out Section? section))
            {
                AssembleObjectSection(section!);
            }
        }
    }

    private void AssembleObjectSection(Section section)
    {
        int offset = _code.Count;
        foreach (int position in section.AbsolutePositionsToBeOffset)
        {
            section.Code[position] += (byte)offset;
            section.Code[position + 1] += (byte)(offset >> 8);
        }

        foreach (Label label in section.Labels)
        {
            _labels.Add(label.Name, label.Location + offset);
        }
        foreach (Label label in section.ExternalLabels)
        {
            _externalLabels.Add(new Label(label.Name, label.Location + offset));
        }
        _code.AddRange(section.Code);
    }

    private void ResolveExternalLabels()
    {
        foreach (Label label in _externalLabels)
        {
            if (!_labels.TryGetValue(label.Name, out int value)) throw new Exception();

            _code[label.Location] = (byte)value;
            _code[label.Location + 1] = (byte)(value >> 8);
        }
    }
}