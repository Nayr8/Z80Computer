using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z80Linker;
public class SectionHeader
{
    public int SectionOffset { get; }
    public int SectionLength { get; }
    public string SectionName { get; }

    public SectionHeader(int sectionOffset, int sectionLength, string sectionName)
    {
        SectionOffset = sectionOffset;
        SectionLength = sectionLength;
        SectionName = sectionName;
    }
}
