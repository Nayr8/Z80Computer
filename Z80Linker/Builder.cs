namespace Z80Linker;

public class Builder
{
    private List<ObjectFile> _objectFiles = new();
    private BuildFile _buildFile;
    private List<byte> _code = new();

    public void Build()
    {
        foreach (KeyValuePair<int, List<string>> buildLocation in _buildFile.BuildOrder)
        {
            while (_code.Count < buildLocation.Key)
                _code.Add(0x00);

            foreach (string section in buildLocation.Value) 
                BuildSection(section);
        }
    }

    private void BuildSection(string section)
    {
        foreach (ObjectFile objectFile in _objectFiles.Where(objectFile => objectFile.SectionExists(section)))
        {
            
        }
    }
}