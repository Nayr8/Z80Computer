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

        private const int KernelRamSize = 0x800; // 512B
        private readonly byte[] _kernelRom = new byte[0x2000 - KernelRamSize];
        private readonly byte[] _kernelRam = new byte[KernelRamSize];
        private readonly byte[] _memory = new byte[0x200000]; // 2 MiB
        
        public byte ReadByte(int address)
        {
            if (address < 0x2000)
            {
                if (address < 0x1800) return _kernelRom[address]; // Kernel ROM
                return _kernelRam[address - 0x1800];
            }
            int offset = PageTable[address / 0x2000 - 1] * 0x2000;

            return _memory[address + offset];
        }

        public void WriteByte(int address, byte value)
        {
            if (address < 0x2000) {
                if (address >= 0x1800) _kernelRam[address - 0x1800] = value; // Kernel ROM
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
