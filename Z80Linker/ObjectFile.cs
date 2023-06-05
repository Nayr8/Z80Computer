
namespace Z80Linker;
public class ObjectFile
{
    private byte[] _data;
    private List<SectionHeader> _headers = new();
    private Dictionary<string, LabelLocation> _labels = new();
    private Dictionary<string, LabelLocation> _labelAbsoluteDestination = new();
    private Dictionary<string, LabelLocation> _labelRelativeDestination = new();
    private Dictionary<string, List<int>> _positionsToBeOffsetBySectionPosition = new();

    public ObjectFile(byte[] data)
    {
        _data = data;
        Parse();
    }

    private void Parse()
    {

    }
}
