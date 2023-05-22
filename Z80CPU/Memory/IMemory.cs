using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z80CPUEmulator.Memory
{
    public interface IMemory
    {
        public byte ReadByte(int address);
        public void WriteByte(int address, byte value);
    }
}
