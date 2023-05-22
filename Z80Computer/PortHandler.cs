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
            MemoryPage0 = 0x01, // 0x0000 - 0x1FFF
            MemoryPage1 = 0x02, // 0x2000 - 0x3FFF
            MemoryPage2 = 0x03, // 0x4000 - 0x5FFF
            MemoryPage3 = 0x04, // 0x6000 - 0x7FFF
            MemoryPage4 = 0x05, // 0x8000 - 0x9FFF
            MemoryPage5 = 0x06, // 0xA000 - 0xBFFF
            MemoryPage6 = 0x07, // 0xC000 - 0xDFFF
            MemoryPage7 = 0x08, // 0xE000 - 0xFFFF

            WriteByte = 0x10;

        public PortHandler(Memory memory)
        {
            _memory = memory;
        }

        public byte Read(byte port)
        {
            return port switch
            {
                (>=MemoryPage0) and (<=MemoryPage7) => _memory.PageTable[port - MemoryPage0],
                WriteByte => 0,
                _ => 0,
            };
        }

        public void Write(byte port, byte value)
        {
            switch (port) {
                case (>= MemoryPage0) and (<= MemoryPage7): _memory.PageTable[port - MemoryPage0] = value; break;
                case WriteByte: Console.Write((char)value); break;
            }
        }
    }
}
