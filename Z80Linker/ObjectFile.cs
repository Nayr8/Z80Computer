namespace Z80Linker;

public class ObjectFile
{
    public Dictionary<string, Section?> Sections = new();

    public ObjectFile(byte[] file)
    {
        int sectionEntries = file[7];
        int nextSection = 8;
        for (int i = 0; i < sectionEntries; ++i)
        {
            Section? section = new(file, nextSection);
            nextSection = section.NextSectionOffset;
            Sections.Add(section.Name, section);
        }
    }
}