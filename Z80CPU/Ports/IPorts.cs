using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z80CPUEmulator.Ports
{
    public interface IPorts
    {
        void Write(byte port, byte value);
        byte Read(byte port);
    }
}
