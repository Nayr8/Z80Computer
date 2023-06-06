namespace Z80Linker;

public class ObjectFile
{
    public List<Section> Sections;

    public ObjectFile(byte[] file)
    {
        int sectionEntries = file[7];
        int nextSection = 8;
        for (int i = 0; i < sectionEntries; ++i)
        {
            Section section = new(file, nextSection);
            nextSection = section.NextSectionOffset;
            Sections.Add(section);
        }
    }
}