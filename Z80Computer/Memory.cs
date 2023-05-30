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
            0, 1, 2, 3, 4, 5, 6
        };

        private readonly byte[] _memory = new byte[0x200000]; // 2 MiB
        private readonly byte[] _kernelRom = new byte[0x2000];
        
        public byte ReadByte(int address)
        {
            if (address < 0x2000)
            {
                return _kernelRom[address]; // Kernel ROM
            }
            int offset = PageTable[address / 0x2000 - 1] * 0x2000;

            return _memory[address + offset];
        }

        public void WriteByte(int address, byte value)
        {
            if (address < 0x2000)
            {
                return; // ROM
            }
            int offset = PageTable[address / 0x2000 - 1] * 0x2000;

            _memory[address + offset] = value;
        }

        public void WriteToRom(int address, byte[] values)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                _kernelRom[address + i] = values[i];
            }
        }
    }
}
