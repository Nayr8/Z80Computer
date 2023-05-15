using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z80CPUEmulator.Instructions
{
    public static class MiscOpcode
    {
        public const byte
            InputFromPortCToB = 0x40,
            OutputBToPortC = 0x41,
            SubCarryBCFromHL = 0x42,
            StoreBCInImmediateAddress = 0x43,
            NegateA = 0x44,
            ReturnNonMaskableInterrupt = 0x45,
            SetInterruptMode0 = 0x46,
            StoreAInInterruptVector = 0x47,
            InputFromPortCToC = 0x48,
            OutputCToPortC = 0x49,
            AddCarryBCToHL = 0x4A,
            LoadBCFromImmediateAddress = 0x4B,
            ReturnMaskableInterrupt = 0x4D,
            StoreAInRefreshCounter = 0x4F,

            InputFromPortCToD = 0x50,
            OutputDToPortC = 0x51,
            SubCarryDEFromHL = 0x52,
            StoreDEInImmediateAddress = 0x53,
            SetInterruptMode1 = 0x56,
            LoadInterruptVectorToA = 0x57,
            InputFromPortCToE = 0x58,
            OutputEToPortC = 0x59,
            AddCarryDEToHL = 0x5A,
            LoadDEFromImmediateAddress = 0x5B,
            SetInterruptMode2 = 0x5E,
            LoadRefreshCounterToA = 0x5F,

            InputFromPortCToH = 0x60,
            OutputHToPortC = 0x61,
            SubCarryHLFromHL = 0x62,
            StoreHLInImmediateAddress = 0x63,
            RotateRightDecimal = 0x67,
            InputFromPortCToL = 0x68,
            OutputAToPortL = 0x69,
            AddCarryHLToHL = 0x6A,
            LoadHLFromImmediateAddress = 0x6B,
            LeftRotateDecimal = 0x6F,

            InputFromCFlagsOnly = 0x70,
            Output0ToPortC = 0x71,
            SubCarryStackPointerFromHL = 0x72,
            StoreStackPointerInImmediateAddress = 0x73,
            InputFromPortCToA = 0x78,
            OutputAToPortC = 0x79,
            AddCarryStackPointerToHL = 0x7A,
            LoadSPFromImmediateAddress = 0x7B,

            MoveIncrement = 0xA0,
            CompareIncrement = 0xA1,
            InputIncrement = 0xA2,
            OuputIncrement = 0xA3,
            MoveDecrement = 0xA8,
            CompareDecrement = 0xA9,
            InputDecrement = 0xAA,
            OutputDecrement = 0xAB,

            MoveIncrementRepeat = 0xB0,
            CompareIncrementRepeat = 0xB1,
            InputIncrementRepeat = 0xB2,
            OutputIncrementRepeat = 0xB3,
            MoveDecrementRepeat = 0xB8,
            CompareDecrementRepeat = 0xB9,
            InputDecrementRepeat = 0xBA,
            OutputDecrementRepeat = 0xBC;
    }
}
