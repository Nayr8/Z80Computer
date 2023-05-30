using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z80CPUEmulator.Ports;

namespace Z80Computer
{
    public class PortHandler : IPorts
    {
        private Memory _memory;

        public const byte
            MemoryPage1 = 0x01, // 0x2000 - 0x3FFF
            MemoryPage2 = 0x02, // 0x4000 - 0x5FFF
            MemoryPage3 = 0x03, // 0x6000 - 0x7FFF
            MemoryPage4 = 0x04, // 0x8000 - 0x9FFF
            MemoryPage5 = 0x05, // 0xA000 - 0xBFFF
            MemoryPage6 = 0x06, // 0xC000 - 0xDFFF
            MemoryPage7 = 0x07, // 0xE000 - 0xFFFF

            WriteByte = 0x10;

        public PortHandler(Memory memory)
        {
            _memory = memory;
        }

        public byte Read(byte port)
        {
            return port switch
            {
                (>=MemoryPage1) and (<=MemoryPage7) => _memory.PageTable[port - MemoryPage1],
                WriteByte => 0,
                _ => 0,
            };
        }

        public void Write(byte port, byte value)
        {
            switch (port) {
                case (>= MemoryPage1) and (<= MemoryPage7): _memory.PageTable[port - MemoryPage1] = value; break;
                case WriteByte: Console.Write((char)value); break;
            }
        }
    }
}
