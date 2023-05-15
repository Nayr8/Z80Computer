using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z80CPUEmulator.Instructions
{
    public enum LoopedInstruction
    {
        MoveIncrement,
        CompareIncrement,
        InputIncrement,
        OuputIncrement,
        MoveDecrement,
        CompareDecrement,
        InputDecrement,
        OutputDecrement,
    }
}
