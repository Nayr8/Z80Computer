using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z80CPUEmulator.Memory;

namespace Z80Computer
{
    public class Memory : IMemory
    {
        public readonly byte[] PageTable = {
            0, 1, 2, 3, 4, 5, 6, 7
        };

        private readonly byte[] _memory = new byte[0x200000]; // 2 MiB

        public byte ReadByte(int address)
        {
            int offset = PageTable[address / 0x2000] * 0x2000;

            return _memory[address + offset];
        }

        public void WriteByte(int address, byte value)
        {
            int offset = PageTable[address / 0x2000] * 0x2000;

            _memory[address + offset] = value;
        }
    }
}
