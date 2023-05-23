using Z80Assembler.Exceptions;
using Z80Assembler.Operands;

namespace Z80Assembler
{
    public class Statement
    {
        public string? Label;
        public string Instruction;
        public IOperand[] Operands;

        public Statement(string? label, string instruction, IOperand[] operands)
        {
            Label = label;
            Instruction = instruction;
            Operands = operands;
        }

        public Byte[] ToBytes()
        {
            Byte[] result = Instruction switch
            {
                "nop" => NoOperation(),
                "ld" => Load(),
                "inc" => Increment(),
                "dec" => Decrement(),
                "rlca" => RotateALeftCarry(),
                "ex" => Exchange(),
                "add" => Add(),
                "rrca" => RotateARightCarry(),
                "djnz" => Loop(),
                "rla" => RotateALeft(),
                "jr" => JumpRelative(),
                "rra" => RotateARight(),
                "daa" => AdjustBcd(),
                "cpl" => InvertA(),
                "scf" => SetCarry(),
                "ccf" => InvertCarry(),
                "halt" => Halt(),
                "adc" => AddCarry(),
                "sub" => Sub(),
                "sbc" => SubCarry(),
                "and" => And(),
                "xor" => XOr(),
                "or" => Or(),
                "cp" => Compare(),
                "ret" => Return(),
                "pop" => Pop(),
                "jp" => JumpAbsolute(),
                "call" => Call(),
                "push" => Push(),
                "rst" => Reset(),
                "out" => Output(),
                "exx" => ExchangeOther(),
                "in" => Input(),
                "di" => DisableInterrupts(),
                "ei" => EnableInterrupts(),
                "rlc" => RotateLeftCarry(),
                "rrc" => RotateRightCarry(),
                "rl" => RotateLeft(),
                "rr" => RotateRight(),
                "sla" => ShiftLeft(),
                "sra" => ShiftRight(),
                "sll" => ShiftLeft1(),
                "srl" => ShiftRight0(),
                "bit" => BitTest(),
                "res" => ResetBit(),
                "set" => SetBit(),
                "neg" => Negate(),
                "retn" => ReturnNonMaskableInterrupt(),
                "im" => InterruptMode(),
                "reti" => ReturnMaskableInterrupt(),
                "rrd" => RotateRightDecimal(),
                "rld" => RotateLeftDecimal(),
                "ldi" => MoveIncrement(),
                "cpi" => CompareIncrement(),
                "ini" => InputIncrement(),
                "outi" => OutputIncrement(),
                "ldd" => MoveDecrement(),
                "cpd" => CompareDecrement(),
                "ind" => InputDecrement(),
                "outd" => OutputDecrement(),
                "ldir" => MoveIncrementRepeat(),
                "cpir" => CompareIncrementRepeat(),
                "inir" => InputIncrementRepeat(),
                "otir" => OutputIncrementRepeat(),
                "lddr" => MoveDecrementRepeat(),
                "cpdr" => CompareDecrementRepeat(),
                "indr" => InputDecrementRepeat(),
                "otdr" => OutputDecrementRepeat(),
                _ => throw new Exception()
            };
            result[0].Label = Label;
            return result;
        }

        private Byte[] NoOperation()
        {
            AssertOperandCount0("nop");
            return Byte.Bytes(0x00);
        }
        private Byte[] Load()
        {
            
        }
        private Byte[] Increment()
        {
            
        }
        private Byte[] Decrement()
        {
            
        }
        private Byte[] RotateALeftCarry()
        {
            AssertOperandCount0("rlca");
            return Byte.Bytes(0x07);
        }
        private Byte[] Exchange()
        {
            
        }
        private Byte[] Add()
        {
            
        }
        private Byte[] RotateARightCarry()
        {
            AssertOperandCount0("rrca");
            return Byte.Bytes(0x0F);
        }
        private Byte[] Loop()
        {
            
        }
        private Byte[] RotateALeft()
        {
            AssertOperandCount0("rla");
            return Byte.Bytes(0x17);
        }
        private Byte[] JumpRelative()
        {
            
        }
        private Byte[] RotateARight()
        {
            AssertOperandCount0("rra");
            return Byte.Bytes(0x1F);
        }
        private Byte[] AdjustBcd()
        {
            AssertOperandCount0("daa");
            return Byte.Bytes(0x27);
        }
        private Byte[] InvertA()
        {
            AssertOperandCount0("cpl");
            return Byte.Bytes(0x2F);
        }
        private Byte[] SetCarry()
        {
            AssertOperandCount0("scf");
            return Byte.Bytes(0x37);
        }
        private Byte[] InvertCarry()
        {
            AssertOperandCount0("ccf");
            return Byte.Bytes(0x3F);
        }

        private Byte[] Halt()
        {
            AssertOperandCount0("halt");
            return Byte.Bytes(0x76);
        }
        private Byte[] AddCarry()
        {
            
        }
        private Byte[] Sub()
        {
            
        }
        private Byte[] SubCarry()
        {
            
        }
        private Byte[] And()
        {
            
        }
        private Byte[] XOr()
        {
            
        }
        private Byte[] Or()
        {
            
        }
        private Byte[] Compare()
        {
            
        }
        private Byte[] Return()
        {
            
        }
        private Byte[] Pop()
        {
            
        }
        private Byte[] JumpAbsolute()
        {
            
        }
        private Byte[] Call()
        {
            
        }
        private Byte[] Push()
        {
            
        }
        private Byte[] Reset()
        {
            
        }
        private Byte[] Output()
        {
            
        }
        private Byte[] ExchangeOther()
        {
            AssertOperandCount0("exx");
            return Byte.Bytes(0xD9);
        }
        private Byte[] Input()
        {
            
        }
        private Byte[] DisableInterrupts()
        {
            AssertOperandCount0("di");
            return Byte.Bytes(0xF3);
        }
        private Byte[] EnableInterrupts()
        {
            AssertOperandCount0("di");
            return Byte.Bytes(0xFB);
        }

        private Byte[] RotateLeftCarry()
        {
            
        }
        private Byte[] RotateRightCarry()
        {
            
        }
        private Byte[] RotateLeft()
        {
            
        }
        private Byte[] RotateRight()
        {
            
        }

        private Byte[] ShiftLeft()
        {
            
        }

        private Byte[] ShiftRight()
        {
            
        }

        private Byte[] ShiftLeft1()
        {
            
        }

        private Byte[] ShiftRight0()
        {
            
        }

        private Byte[] BitTest()
        {
            
        }

        private Byte[] ResetBit()
        {
            
        }

        private Byte[] SetBit()
        {
            
        }

        private Byte[] Negate()
        {
            AssertOperandCount0("neg");
            return Byte.Bytes(0xED, 0x44);
        }

        private Byte[] ReturnNonMaskableInterrupt()
        {
            AssertOperandCount0("retn");
            return Byte.Bytes(0xED, 0x45);
        }

        private Byte[] InterruptMode()
        {
            
        }

        private Byte[] ReturnMaskableInterrupt()
        {
            AssertOperandCount0("reti");
            return Byte.Bytes(0xED, 0x4D);
        }

        private Byte[] RotateRightDecimal()
        {
            AssertOperandCount0("rrd");
            return Byte.Bytes(0xED, 0x67);
        }

        private Byte[] RotateLeftDecimal()
        {
            AssertOperandCount0("rld");
            return Byte.Bytes(0xED, 0x6F);
        }

        private Byte[] MoveIncrement()
        {
            AssertOperandCount0("ldi");
            return Byte.Bytes(0xED, 0xA0);
        }
        private Byte[] CompareIncrement() {
            AssertOperandCount0("cpi");
            return Byte.Bytes(0xED, 0xA1);
        }
        private Byte[] InputIncrement() {
            AssertOperandCount0("ini");
            return Byte.Bytes(0xED, 0xA2);
        }
        private Byte[] OutputIncrement() {
            AssertOperandCount0("outi");
            return Byte.Bytes(0xED, 0xA3);
        }
        private Byte[] MoveDecrement () {
            AssertOperandCount0("ldd");
            return Byte.Bytes(0xED, 0xA8);
        }
        private Byte[] CompareDecrement() {
            AssertOperandCount0("cpd");
            return Byte.Bytes(0xED, 0xA9);
        }
        private Byte[] InputDecrement() {
            AssertOperandCount0("ind");
            return Byte.Bytes(0xED, 0xAA);
        }
        private Byte[] OutputDecrement() {
            AssertOperandCount0("outd");
            return Byte.Bytes(0xED, 0xAB);
        }

        private Byte[] MoveIncrementRepeat() {
            AssertOperandCount0("cpir");
            return Byte.Bytes(0xED, 0xB0);
        }

        private Byte[] CompareIncrementRepeat()
        {
            AssertOperandCount0("cpir");
            return Byte.Bytes(0xED, 0xB1);
        }

        private Byte[] InputIncrementRepeat()
        {
            AssertOperandCount0("inir");
            return Byte.Bytes(0xED, 0xB2);
        }

        private Byte[] OutputIncrementRepeat()
        {
            AssertOperandCount0("otir");
            return Byte.Bytes(0xED, 0xB3);
        }

        private Byte[] MoveDecrementRepeat()
        {
            AssertOperandCount0("lddr");
            return Byte.Bytes(0xED, 0xB8);
        }

        private Byte[] CompareDecrementRepeat()
        {
            AssertOperandCount0("cpdr");
            return Byte.Bytes(0xED, 0xB9);
        }

        private Byte[] InputDecrementRepeat()
        {
            AssertOperandCount0("indr");
            return Byte.Bytes(0xED, 0xBA);
        }

        private Byte[] OutputDecrementRepeat()
        {
            AssertOperandCount0("otdr");
            return Byte.Bytes(0xED, 0xBB);
        }



        private void AssertOperandCount0(string instruction)
        {
            if (Operands.Length != 0)
            {
                throw new IncorrectNumberOfOperandsException(instruction, Operands.Length, new HashSet<int>{ 0 });
            }
        }

        private void AssertOperandCount1(string instruction)
        {
            if (Operands.Length != 1)
            {
                throw new IncorrectNumberOfOperandsException(instruction, Operands.Length, new HashSet<int> { 1 });
            }
        }

        private void AssertOperandCount2(string instruction)
        {
            if (Operands.Length != 2)
            {
                throw new IncorrectNumberOfOperandsException(instruction, Operands.Length, new HashSet<int> { 2 });
            }
        }

        private void AssertOperandCount0Or1(string instruction)
        {
            if (Operands.Length != 0 && Operands.Length != 1)
            {
                throw new IncorrectNumberOfOperandsException(instruction, Operands.Length, new HashSet<int> { 0, 1 });
            }
        }

        private void AssertOperandCount1Or2(string instruction)
        {
            if (Operands.Length != 1 && Operands.Length != 2)
            {
                throw new IncorrectNumberOfOperandsException(instruction, Operands.Length, new HashSet<int> { 1, 2 });
            }
        }

        private void AssertOperandCount0Or1Or2(string instruction)
        {
            if (Operands.Length != 0 && Operands.Length != 1 && Operands.Length != 2)
            {
                throw new IncorrectNumberOfOperandsException(instruction, Operands.Length, new HashSet<int>{ 0, 1, 2 });
            }
        }
    }
}
