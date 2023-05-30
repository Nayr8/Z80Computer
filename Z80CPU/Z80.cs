

using System;
using System.Collections;
using Z80CPUEmulator.Instructions;
using Z80CPUEmulator.Memory;
using Z80CPUEmulator.Ports;

namespace Z80CPUEmulator
{
    public class Z80
    {
        public Registers Registers { get; } = new();
        public IMemory Memory { get; }
        public IPorts Ports { get; }
        private InterruptHandler _interruptHandler;
        private int _instructionStart;
        public bool Halted { get; set; }
        private LoopedInstruction? _loopedInstruction = null;

        public Z80(IMemory memory, IPorts ports)
        {
            Memory = memory;
            Ports = ports;
            _interruptHandler = new InterruptHandler(this);

        }

        public void ExecuteNextInstruction()
        {
            if (_loopedInstruction != null)
            {
                HandleLoopedInstruction();
                return;
            }

            _instructionStart = Registers.ProgramCounter;
            byte opcode = ReadNextByte();

            ExecuteInstruction(opcode);
        }

        public void ExecuteInstruction(byte opcode)
        {
            switch (opcode)
            {
                // 0x00
                case Opcode.NoOperation: break;
                case Opcode.LoadImmediateToBC: Registers.BC = ReadNextWord(); break;
                case Opcode.StoreAToAddressInBC: Memory.WriteByte(Registers.BC, Registers.A); break;
                case Opcode.IncrementBC: Registers.BC = Increment(Registers.BC); break;
                case Opcode.IncrementB: Registers.B = Increment(Registers.B); break;
                case Opcode.DecrementB: Registers.B = Decrement(Registers.B); break;
                case Opcode.LoadImmediateToB: Registers.B = ReadNextByte(); break;
                case Opcode.LeftRotateA: Registers.A = LeftRotate(Registers.A); break;
                case Opcode.ExchangeAFWithShadow: Registers.ExchangeAF(); break;
                case Opcode.AddBCToHL: Registers.HL = Add(Registers.HL, Registers.BC); break;
                case Opcode.LoadAFromAddressInBC: Registers.A = Memory.ReadByte(Registers.BC); break;
                case Opcode.DecrementBC: Registers.BC = Decrement(Registers.BC); break;
                case Opcode.IncrementC: Registers.C = Increment(Registers.C); break;
                case Opcode.DecrementC: Registers.C = Decrement(Registers.C); break;
                case Opcode.LoadImmediateToC: Registers.C = ReadNextByte(); break;
                case Opcode.RightRotateA: Registers.A = RightRotate(Registers.A); break;

                // 0x10
                case Opcode.Loop: Loop((sbyte)ReadNextByte()); break;
                case Opcode.LoadImmediateToDE: Registers.DE = ReadNextWord(); break;
                case Opcode.StoreAToAddressInDE: Memory.WriteByte(Registers.DE, Registers.A); break;
                case Opcode.IncrementDE: Registers.DE = Increment(Registers.DE); break;
                case Opcode.IncrementD: Registers.D = Increment(Registers.D); break;
                case Opcode.DecrementD: Registers.D = Decrement(Registers.D); break;
                case Opcode.LoadImmediateToD: Registers.D = ReadNextByte(); break;
                case Opcode.LeftRotateA9Bit: Registers.A = LeftRotate9Bit(Registers.A); break;
                case Opcode.JumpRelative: JumpRelativeIf(true, (sbyte)ReadNextByte()); break;
                case Opcode.AddDEToHL: Registers.HL = Add(Registers.HL, Registers.DE); break;
                case Opcode.LoadAFromAddressInDE: Registers.A = Memory.ReadByte(Registers.DE); break;
                case Opcode.DecrementDE: Registers.DE = Decrement(Registers.DE); break;
                case Opcode.IncrementE: Registers.E = Increment(Registers.E); break;
                case Opcode.DecrementE: Registers.E = Decrement(Registers.E); break;
                case Opcode.LoadImmediateToE: Registers.E = ReadNextByte(); break;
                case Opcode.RightRotateA9Bit: Registers.A = RightRotate9Bit(Registers.A); break;

                // 0x20
                case Opcode.JumpRelativeIfNotZero: JumpRelativeIf(!Registers.Flags.Zero, (sbyte)ReadNextByte()); break;
                case Opcode.LoadImmediateToHL: Registers.HL = ReadNextWord(); break;
                case Opcode.StoreHLToImmediateAddress: WriteWord(ReadNextWord(), Registers.HL); break;
                case Opcode.IncrementHL: Registers.HL = Increment(Registers.HL); break;
                case Opcode.IncrementH: Registers.H = Increment(Registers.H); break;
                case Opcode.DecrementH: Registers.H = Decrement(Registers.H); break;
                case Opcode.LoadImmediateToH: Registers.H = ReadNextByte(); break;
                case Opcode.AdjustAForBCDAddAndSub: AdjustAForBCDAddAndSub(); break;
                case Opcode.JumpRelativeIfZero: JumpRelativeIf(Registers.Flags.Zero, (sbyte)ReadNextByte()); break;
                case Opcode.AddHLToHL: Registers.HL = Add(Registers.HL, Registers.HL); break;
                case Opcode.LoadHLFromImmediateAddress: Registers.HL = ReadWordFromAddress(ReadNextWord()); break;
                case Opcode.DecrementHL: Registers.HL = Decrement(Registers.HL); break;
                case Opcode.IncrementL: Registers.L = Increment(Registers.L); break;
                case Opcode.DecrementL: Registers.L = Decrement(Registers.L); break;
                case Opcode.LoadImmediateToL: Registers.L = ReadNextByte(); break;
                case Opcode.InvertA: Registers.A = Invert(Registers.A); break;

                // 0x30
                case Opcode.JumpRelativeIfNotCarry: JumpRelativeIf(!Registers.Flags.Carry, (sbyte)ReadNextByte()); break;
                case Opcode.LoadImmediateToSP: Registers.StackPointer = ReadNextWord(); break;
                case Opcode.StoreAInImmediateAddress: Memory.WriteByte(ReadNextWord(), Registers.A); break;
                case Opcode.IncrementSP: Registers.StackPointer = Increment(Registers.StackPointer); break;
                case Opcode.IncrementAddressStoredInHl: Memory.WriteByte(Registers.HL, Increment(Memory.ReadByte(Registers.HL))); break;
                case Opcode.DecrementAddressStoresInHl: Memory.WriteByte(Registers.HL, Decrement(Memory.ReadByte(Registers.HL))); break;
                case Opcode.LoadImmediateToAddressStoredInHL: Memory.WriteByte(Registers.HL, ReadNextByte()); break;
                case Opcode.SetCarryFlag: Registers.Flags.Carry = true; break;
                case Opcode.JumpRelativeIfCarry: JumpRelativeIf(Registers.Flags.Carry, (sbyte)ReadNextByte()); break;
                case Opcode.AddSPToHL: Registers.HL = Add(Registers.HL, Registers.StackPointer); break;
                case Opcode.LoadAFromImmediateAddress: Registers.A = Memory.ReadByte(ReadNextWord()); break;
                case Opcode.DecrementSP: Registers.StackPointer = Decrement(Registers.StackPointer); break;
                case Opcode.IncrementA: Registers.A = Increment(Registers.A); break;
                case Opcode.DecrementA: Registers.A = Decrement(Registers.A); break;
                case Opcode.LoadImmediateToA: Registers.A = ReadNextByte(); break;
                case Opcode.InvertCarry: Registers.Flags.Carry = !Registers.Flags.Carry; break;

                // 0x40
                case Opcode.LoadBFromB: Registers.B = Registers.B; break;
                case Opcode.LoadBFromC: Registers.B = Registers.C; break;
                case Opcode.LoadBFromD: Registers.B = Registers.D; break;
                case Opcode.LoadBFromE: Registers.B = Registers.E; break;
                case Opcode.LoadBFromH: Registers.B = Registers.H; break;
                case Opcode.LoadBFromL: Registers.B = Registers.L; break;
                case Opcode.LoadBFromAddressStoredInHL: Registers.B = Memory.ReadByte(Registers.HL); break;
                case Opcode.LoadBFromA: Registers.B = Registers.A; break;
                case Opcode.LoadCFromB: Registers.C = Registers.B; break;
                case Opcode.LoadCFromC: Registers.C = Registers.C; break;
                case Opcode.LoadCFromD: Registers.C = Registers.D; break;
                case Opcode.LoadCFromE: Registers.C = Registers.E; break;
                case Opcode.LoadCFromH: Registers.C = Registers.H; break;
                case Opcode.LoadCFromL: Registers.C = Registers.L; break;
                case Opcode.LoadCFromAddressStoredInHL: Registers.C = Memory.ReadByte(Registers.HL); break;
                case Opcode.LoadCFromA: Registers.C = Registers.A; break;

                // 0x50
                case Opcode.LoadDFromB: Registers.D = Registers.B; break;
                case Opcode.LoadDFromC: Registers.D = Registers.C; break;
                case Opcode.LoadDFromD: Registers.D = Registers.D; break;
                case Opcode.LoadDFromE: Registers.D = Registers.E; break;
                case Opcode.LoadDFromH: Registers.D = Registers.H; break;
                case Opcode.LoadDFromL: Registers.D = Registers.L; break;
                case Opcode.LoadDFromAddressStoredInHL: Registers.D = Memory.ReadByte(Registers.HL); break;
                case Opcode.LoadDFromA: Registers.D = Registers.A; break;
                case Opcode.LoadEFromB: Registers.E = Registers.B; break;
                case Opcode.LoadEFromC: Registers.E = Registers.C; break;
                case Opcode.LoadEFromD: Registers.E = Registers.D; break;
                case Opcode.LoadEFromE: Registers.E = Registers.E; break;
                case Opcode.LoadEFromH: Registers.E = Registers.H; break;
                case Opcode.LoadEFromL: Registers.E = Registers.L; break;
                case Opcode.LoadEFromAddressStoredInHL: Registers.E = Memory.ReadByte(Registers.HL); break;
                case Opcode.LoadEFromA: Registers.E = Registers.A; break;

                // 0x60
                case Opcode.LoadHFromB: Registers.H = Registers.B; break;
                case Opcode.LoadHFromC: Registers.H = Registers.C; break;
                case Opcode.LoadHFromD: Registers.H = Registers.D; break;
                case Opcode.LoadHFromE: Registers.H = Registers.E; break;
                case Opcode.LoadHFromH: Registers.H = Registers.H; break;
                case Opcode.LoadHFromL: Registers.H = Registers.L; break;
                case Opcode.LoadHFromAddressStoredInHL: Registers.H = Memory.ReadByte(Registers.HL); break;
                case Opcode.LoadHFromA: Registers.H = Registers.A; break;
                case Opcode.LoadLFromB: Registers.L = Registers.B; break;
                case Opcode.LoadLFromC: Registers.L = Registers.C; break;
                case Opcode.LoadLFromD: Registers.L = Registers.D; break;
                case Opcode.LoadLFromE: Registers.L = Registers.E; break;
                case Opcode.LoadLFromH: Registers.L = Registers.H; break;
                case Opcode.LoadLFromL: Registers.L = Registers.L; break;
                case Opcode.LoadLFromAddressStoredInHL: Registers.L = Memory.ReadByte(Registers.HL); break;
                case Opcode.LoadLFromA: Registers.L = Registers.A; break;

                // 0x70
                case Opcode.StoreToAddressStoredInHLFromB: Memory.WriteByte(Registers.H, Registers.B); break;
                case Opcode.StoreToAddressStoredInHLFromC: Memory.WriteByte(Registers.H, Registers.C); break;
                case Opcode.StoreToAddressStoredInHLFromD: Memory.WriteByte(Registers.H, Registers.D); break;
                case Opcode.StoreToAddressStoredInHLFromE: Memory.WriteByte(Registers.H, Registers.E); break;
                case Opcode.StoreToAddressStoredInHLFromH: Memory.WriteByte(Registers.H, Registers.H); break;
                case Opcode.StoreToAddressStoredInHLFromL: Memory.WriteByte(Registers.H, Registers.L); break;
                case Opcode.Halt: Halted = true; break;
                case Opcode.StoreToAddressStoredInHLFromA: Memory.WriteByte(Registers.H, Registers.A); break;
                case Opcode.LoadAFromB: Registers.A = Registers.B; break;
                case Opcode.LoadAFromC: Registers.A = Registers.C; break;
                case Opcode.LoadAFromD: Registers.A = Registers.D; break;
                case Opcode.LoadAFromE: Registers.A = Registers.E; break;
                case Opcode.LoadAFromH: Registers.A = Registers.H; break;
                case Opcode.LoadAFromL: Registers.A = Registers.L; break;
                case Opcode.LoadAFromAddressStoredInHL: Registers.A = Memory.ReadByte(Registers.HL); break;
                case Opcode.LoadAFromA: Registers.A = Registers.A; break;

                // 0x80
                case Opcode.AddBToA: Registers.A = Add(Registers.A, Registers.B); break;
                case Opcode.AddCToA: Registers.A = Add(Registers.A, Registers.C); break;
                case Opcode.AddDToA: Registers.A = Add(Registers.A, Registers.D); break;
                case Opcode.AddEToA: Registers.A = Add(Registers.A, Registers.E); break;
                case Opcode.AddHToA: Registers.A = Add(Registers.A, Registers.H); break;
                case Opcode.AddLToA: Registers.A = Add(Registers.A, Registers.L); break;
                case Opcode.AddAddressFromHLToA: Registers.A = Add(Registers.A, Memory.ReadByte(Registers.HL)); break;
                case Opcode.AddAToA: Registers.A = Add(Registers.A, Registers.A); break;
                case Opcode.AddCarryBToA: Registers.A = AddWithCarry(Registers.A, Registers.B); break;
                case Opcode.AddCarryCToA: Registers.A = AddWithCarry(Registers.A, Registers.C); break;
                case Opcode.AddCarryDToA: Registers.A = AddWithCarry(Registers.A, Registers.D); break;
                case Opcode.AddCarryEToA: Registers.A = AddWithCarry(Registers.A, Registers.E); break;
                case Opcode.AddCarryHToA: Registers.A = AddWithCarry(Registers.A, Registers.H); break;
                case Opcode.AddCarryLToA: Registers.A = AddWithCarry(Registers.A, Registers.L); break;
                case Opcode.AddCarryAddressFromHLToA: Registers.A = AddWithCarry(Registers.A, Memory.ReadByte(Registers.HL)); break;
                case Opcode.AddCarryAToA: Registers.A = AddWithCarry(Registers.A, Registers.A); break;

                // 0x90
                case Opcode.SubBFromA: Registers.A = Sub(Registers.A, Registers.B); break;
                case Opcode.SubCFromA: Registers.A = Sub(Registers.A, Registers.C); break;
                case Opcode.SubDFromA: Registers.A = Sub(Registers.A, Registers.D); break;
                case Opcode.SubEFromA: Registers.A = Sub(Registers.A, Registers.E); break;
                case Opcode.SubHFromA: Registers.A = Sub(Registers.A, Registers.H); break;
                case Opcode.SubLFromA: Registers.A = Sub(Registers.A, Registers.L); break;
                case Opcode.SubAddressFromHLFromA: Registers.A = Sub(Registers.A, Memory.ReadByte(Registers.HL)); break;
                case Opcode.SubAFromA: Registers.A = Sub(Registers.A, Registers.A); break;
                case Opcode.SubCarryBFromA: Registers.A = SubWithCarry(Registers.A, Registers.B); break;
                case Opcode.SubCarryCFromA: Registers.A = SubWithCarry(Registers.A, Registers.C); break;
                case Opcode.SubCarryDFromA: Registers.A = SubWithCarry(Registers.A, Registers.D); break;
                case Opcode.SubCarryEFromA: Registers.A = SubWithCarry(Registers.A, Registers.E); break;
                case Opcode.SubCarryHFromA: Registers.A = SubWithCarry(Registers.A, Registers.H); break;
                case Opcode.SubCarryLFromA: Registers.A = SubWithCarry(Registers.A, Registers.L); break;
                case Opcode.SubCarryAddressFromHLFromA: Registers.A = SubWithCarry(Registers.A, Memory.ReadByte(Registers.HL)); break;
                case Opcode.SubCarryAFromA: Registers.A = SubWithCarry(Registers.A, Registers.A); break;

                // 0xA0
                case Opcode.AndAWithB: Registers.A = And(Registers.A, Registers.B); break;
                case Opcode.AndAWithC: Registers.A = And(Registers.A, Registers.C); break;
                case Opcode.AndAWithD: Registers.A = And(Registers.A, Registers.D); break;
                case Opcode.AndAWithE: Registers.A = And(Registers.A, Registers.E); break;
                case Opcode.AndAWithH: Registers.A = And(Registers.A, Registers.H); break;
                case Opcode.AndAWithL: Registers.A = And(Registers.A, Registers.L); break;
                case Opcode.AndAWithAddressFromHL: Registers.A = And(Registers.A, Memory.ReadByte(Registers.HL)); break;
                case Opcode.AndAWithA: Registers.A = And(Registers.A, Registers.A); break;
                case Opcode.XorAWithB: Registers.A = Xor(Registers.A, Registers.B); break;
                case Opcode.XorAWithC: Registers.A = Xor(Registers.A, Registers.C); break;
                case Opcode.XorAWithD: Registers.A = Xor(Registers.A, Registers.D); break;
                case Opcode.XorAWithE: Registers.A = Xor(Registers.A, Registers.E); break;
                case Opcode.XorAWithH: Registers.A = Xor(Registers.A, Registers.H); break;
                case Opcode.XorAWithL: Registers.A = Xor(Registers.A, Registers.L); break;
                case Opcode.XorAWithAddressFromHL: Registers.A = Xor(Registers.A, Memory.ReadByte(Registers.HL)); break;
                case Opcode.XorAWithA: Registers.A = Xor(Registers.A, Registers.A); break;

                // 0xB0
                case Opcode.OrAWithB: Registers.A = Or(Registers.A, Registers.B); break;
                case Opcode.OrAWithC: Registers.A = Or(Registers.A, Registers.C); break;
                case Opcode.OrAWithD: Registers.A = Or(Registers.A, Registers.D); break;
                case Opcode.OrAWithE: Registers.A = Or(Registers.A, Registers.E); break;
                case Opcode.OrAWithH: Registers.A = Or(Registers.A, Registers.H); break;
                case Opcode.OrAWithL: Registers.A = Or(Registers.A, Registers.L); break;
                case Opcode.OrAWithAddressFromHL: Registers.A = Or(Registers.A, Memory.ReadByte(Registers.HL)); break;
                case Opcode.OrAWithA: Registers.A = Or(Registers.A, Registers.A); break;
                case Opcode.CompareAWithB: Sub(Registers.A, Registers.B); break;
                case Opcode.CompareAWithC: Sub(Registers.A, Registers.C); break;
                case Opcode.CompareAWithD: Sub(Registers.A, Registers.D); break;
                case Opcode.CompareAWithE: Sub(Registers.A, Registers.E); break;
                case Opcode.CompareAWithH: Sub(Registers.A, Registers.H); break;
                case Opcode.CompareAWithL: Sub(Registers.A, Registers.L); break;
                case Opcode.CompareAWithAddressFromHL: Sub(Registers.A, Memory.ReadByte(Registers.HL)); break;
                case Opcode.CompareAWithA: Sub(Registers.A, Registers.A); break;

                // 0xC0
                case Opcode.ReturnIfNotZero: Return(!Registers.Flags.Zero); break;
                case Opcode.PopBC: Registers.BC = PopWord(); break;
                case Opcode.JumpIfNotZeroAbsolute: JumpAbsoluteIf(!Registers.Flags.Zero, ReadNextWord()); break;
                case Opcode.JumpAbsolute: JumpAbsoluteIf(true, ReadNextWord()); break;
                case Opcode.CallIfNotZeroAbsolute: Call(!Registers.Flags.Zero, ReadNextWord()); break;
                case Opcode.PushBC: Push(Registers.BC); break;
                case Opcode.AddImmediateToA: Registers.A = Add(Registers.A, ReadNextByte()); break;
                case Opcode.Reset0x00: Call(true, 0x00); break;
                case Opcode.ReturnIfZero: Return(Registers.Flags.Zero); break;
                case Opcode.Return: Return(true); break;
                case Opcode.JumpIfZeroAbsolute: JumpAbsoluteIf(Registers.Flags.Zero, ReadNextWord()); break;
                case Opcode.BitInstruction: BitInstruction(); break;
                case Opcode.CallIfZeroAbsolute: Call(Registers.Flags.Zero, ReadNextWord()); break;
                case Opcode.CallAbsolute: Call(true, ReadNextWord()); break;
                case Opcode.AddCarryImmediateToA: Registers.A = AddWithCarry(Registers.A, ReadNextByte()); break;
                case Opcode.Reset0x08: Call(true, 0x08); break;

                // 0xD0
                case Opcode.ReturnIfNotCarry: Return(!Registers.Flags.Carry); break;
                case Opcode.PopDE: Registers.DE = PopWord(); break;
                case Opcode.JumpIfNotCarryAbsolute: JumpAbsoluteIf(!Registers.Flags.Carry, ReadNextWord()); break;
                case Opcode.OutputAToPort: Ports.Write(ReadNextByte(), Registers.A); break;
                case Opcode.CallIfNotCarryAbsolute: Call(!Registers.Flags.Carry, ReadNextWord()); break;
                case Opcode.PushDE: Push(Registers.DE); break;
                case Opcode.SubImmediateFromA: Registers.A = Sub(Registers.A, ReadNextByte()); break;
                case Opcode.Reset0x10: Call(true, 0x10); break;
                case Opcode.ReturnIfCarry: Return(Registers.Flags.Carry); break;
                case Opcode.ExchangeBCDEHLWithShadow: Registers.ExchangeBCDEHL(); break;
                case Opcode.JumpIfCarryAbsolute: JumpAbsoluteIf(Registers.Flags.Carry, ReadNextWord()); break;
                case Opcode.InputFromPortToA: Registers.A = Ports.Read(ReadNextByte()); break;
                case Opcode.CallIfCarryAbsolute: Call(Registers.Flags.Carry, ReadNextWord()); break;
                case Opcode.IXInstruction: IXInstruction(); break;
                case Opcode.SubCarryImmediateFromA: Registers.A = SubWithCarry(Registers.A, ReadNextByte()); break;
                case Opcode.Reset0x18: Call(true, 0x18); break;

                // 0xE0
                case Opcode.ReturnIfNotOverflowOrParity: Return(!Registers.Flags.Overflow); break;
                case Opcode.PopHL: Registers.HL = PopWord(); break;
                case Opcode.JumpIfNotOverflowOrParityAbsolute: JumpAbsoluteIf(!Registers.Flags.Overflow, ReadNextWord()); break;
                case Opcode.ExchangeStackTopWithHL: ExchangeStackTopWithHL(); break;
                case Opcode.CallIfNotOverflowOrParityAbsolute: Call(!Registers.Flags.Overflow, ReadNextWord()); break;
                case Opcode.PushHL: Push(Registers.HL); break;
                case Opcode.AndImmediateWithA: Registers.A = And(Registers.A, ReadNextByte()); break;
                case Opcode.Reset0x20: Call(true, 0x20); break;
                case Opcode.ReturnIfOverflowOrParity: Return(Registers.Flags.Overflow); break;
                case Opcode.JumpAddressFromHLAbsolute: JumpAbsoluteIf(true, Registers.HL); break;
                case Opcode.JumpIfOverflowOrParityAbsolute: JumpAbsoluteIf(Registers.Flags.Overflow, ReadNextWord()); break;
                case Opcode.ExchangeDEAndHL: Registers.ExchangeDEAndHL(); break;
                case Opcode.CallIfOverflowOrParityAbsolute: Call(Registers.Flags.Overflow, ReadNextWord()); break;
                case Opcode.MiscInstruction: MiscInstruction(); break;
                case Opcode.XorImmediateWithA: Registers.A = Xor(Registers.A, ReadNextByte()); break;
                case Opcode.Reset0x28: Call(true, 0x28); break;

                // 0xF0
                case Opcode.ReturnIfNotSign: Return(!Registers.Flags.Sign); break;
                case Opcode.PopAF: Registers.AF = PopWord(); break;
                case Opcode.JumpIfNotSignAbsolute: JumpAbsoluteIf(!Registers.Flags.Sign, ReadNextWord()); break;
                case Opcode.DisableInterrupts: _interruptHandler.InterruptsEnabled = false; break;
                case Opcode.CallIfNotSignAbsolute: Call(!Registers.Flags.Sign, ReadNextWord()); break;
                case Opcode.PushAF: Push(Registers.AF); break;
                case Opcode.OrImmediateWithA: Registers.A = Or(Registers.A, ReadNextByte()); break;
                case Opcode.Reset0x30: Call(true, 0x30); break;
                case Opcode.ReturnIfSign: Return(Registers.Flags.Sign); break;
                case Opcode.LoadHLToSP: Registers.StackPointer = Registers.HL; break;
                case Opcode.JumpIfSignAbsolute: JumpAbsoluteIf(Registers.Flags.Sign, ReadNextWord()); break;
                case Opcode.EnableInterrupts: _interruptHandler.InterruptsEnabled = true; break;
                case Opcode.CallIfSignAbsolute: Call(Registers.Flags.Sign, ReadNextWord()); break;
                case Opcode.IYInstruction: IYInstruction(); break;
                case Opcode.CompareImmediateWithA: Sub(Registers.A, ReadNextByte()); break;
                case Opcode.Reset0x38: Call(true, 0x38); break;
            };
        }

        #region BCD
        public void AdjustAForBCDAddAndSub()
        {
            if (!Registers.Flags.AddSubtract)
            {
                if (Registers.Flags.Carry || Registers.A > 0x99)
                {
                    Registers.A += 0x60;
                    Registers.Flags.Carry = true;
                }
                if (Registers.Flags.HalfCarry || (Registers.A & 0x0F) > 0x09)
                {
                    Registers.A += 0x06;
                }
            } else
            {
                if (Registers.Flags.Carry)
                {
                    Registers.A -= 0x60;
                }
                if (Registers.Flags.HalfCarry)
                {
                    Registers.A -= 0x06;
                }
            }
            Registers.Flags.Zero = Registers.A == 0;
            Registers.Flags.HalfCarry = false;
        }
        #endregion

        #region Looped Instructions
        private void HandleLoopedInstruction()
        {
            switch (_loopedInstruction)
            {
                case LoopedInstruction.MoveIncrement: MoveIncrementRepeat(); break;
                case LoopedInstruction.CompareIncrement: CompareIncrementRepeat(); break;
                case LoopedInstruction.InputIncrement: InputIncrementRepeat(); break;
                case LoopedInstruction.OuputIncrement: OutputIncrementRepeat(); break;
                case LoopedInstruction.MoveDecrement: MoveDecrementRepeat(); break;
                case LoopedInstruction.CompareDecrement: CompareDecrementRepeat(); break;
                case LoopedInstruction.InputDecrement: InputDecrementRepeat(); break;
                case LoopedInstruction.OutputDecrement: OutputDecrementRepeat(); break;
            }
        }

        private void MoveIncrementRepeat()
        {
            MoveIncrement();
            _loopedInstruction = Registers.Flags.Overflow ? LoopedInstruction.MoveIncrement : null;
        }
        private void CompareIncrementRepeat()
        {
            CompareIncrement();
            _loopedInstruction = Registers.Flags.Overflow ? LoopedInstruction.CompareIncrement : null;
        }
        private void InputIncrementRepeat()
        {
            InputIncrement();
            _loopedInstruction = Registers.Flags.Overflow ? LoopedInstruction.InputIncrement : null;
        }
        private void OutputIncrementRepeat()
        {
            OutputIncrement();
            _loopedInstruction = Registers.Flags.Overflow ? LoopedInstruction.OuputIncrement : null;
        }
        private void MoveDecrementRepeat()
        {
            MoveDecrement();
            _loopedInstruction = Registers.Flags.Overflow ? LoopedInstruction.MoveDecrement : null;
        }
        private void CompareDecrementRepeat()
        {
            CompareDecrement();
            _loopedInstruction = Registers.Flags.Overflow ? LoopedInstruction.CompareDecrement : null;
        }
        private void InputDecrementRepeat()
        {
            InputDecrement();
            _loopedInstruction = Registers.Flags.Overflow ? LoopedInstruction.InputDecrement : null;
        }
        private void OutputDecrementRepeat()
        {
            OutputDecrement();
            _loopedInstruction = Registers.Flags.Overflow ? LoopedInstruction.OutputDecrement : null;
        }
        #endregion

        #region Interrupts
        private void ReturnFromNonMaskableInterrupt()
        {
            _interruptHandler.ReturnFromInterrupt();
        }

        private void ReturnFromMaskableInterrupt()
        {
            _interruptHandler.ReturnFromInterrupt();
        }
        #endregion

        #region Bit Instructions
        private void BitInstruction()
        {
            byte bitOpcode = ReadNextByte();

            switch (bitOpcode) {
                // 0x00
                case BitOpcode.LeftRotateB: Registers.B = LeftRotate(Registers.B); break;
                case BitOpcode.LeftRotateC: Registers.C = LeftRotate(Registers.C); break;
                case BitOpcode.LeftRotateD: Registers.D = LeftRotate(Registers.D); break;
                case BitOpcode.LeftRotateE: Registers.E = LeftRotate(Registers.E); break;
                case BitOpcode.LeftRotateH: Registers.H = LeftRotate(Registers.H); break;
                case BitOpcode.LeftRotateL: Registers.L = LeftRotate(Registers.L); break;
                case BitOpcode.LeftRotateAddressInHL: Memory.WriteByte(Registers.HL, LeftRotate(Memory.ReadByte(Registers.HL))); break;
                case BitOpcode.LeftRotateA: Registers.A = LeftRotate(Registers.A); break;
                case BitOpcode.RightRotateB: Registers.B = RightRotate(Registers.B); break;
                case BitOpcode.RightRotateC: Registers.C = RightRotate(Registers.C); break;
                case BitOpcode.RightRotateD: Registers.D = RightRotate(Registers.D); break;
                case BitOpcode.RightRotateE: Registers.E = RightRotate(Registers.E); break;
                case BitOpcode.RightRotateH: Registers.H = RightRotate(Registers.H); break;
                case BitOpcode.RightRotateL: Registers.L = RightRotate(Registers.L); break;
                case BitOpcode.RightRotateAddressInHL: Memory.WriteByte(Registers.HL, RightRotate(Memory.ReadByte(Registers.HL))); break;
                case BitOpcode.RightRotateA: Registers.A = RightRotate(Registers.A); break;
                
                // 0x10
                case BitOpcode.LeftRotateB9Bit: Registers.B = LeftRotate(Registers.B); break;
                case BitOpcode.LeftRotateC9Bit: Registers.C = LeftRotate(Registers.C); break;
                case BitOpcode.LeftRotateD9Bit: Registers.D = LeftRotate(Registers.D); break;
                case BitOpcode.LeftRotateE9Bit: Registers.E = LeftRotate(Registers.E); break;
                case BitOpcode.LeftRotateH9Bit: Registers.H = LeftRotate(Registers.H); break;
                case BitOpcode.LeftRotateL9Bit: Registers.L = LeftRotate(Registers.L); break;
                case BitOpcode.LeftRotateAddressInHL9Bit: Memory.WriteByte(Registers.HL, LeftRotate(Memory.ReadByte(Registers.HL))); break;
                case BitOpcode.LeftRotateA9Bit: Registers.A = LeftRotate(Registers.A); break;
                case BitOpcode.RightRotateB9Bit: Registers.B = RightRotate(Registers.B); break;
                case BitOpcode.RightRotateC9Bit: Registers.C = RightRotate(Registers.C); break;
                case BitOpcode.RightRotateD9Bit: Registers.D = RightRotate(Registers.D); break;
                case BitOpcode.RightRotateE9Bit: Registers.E = RightRotate(Registers.E); break;
                case BitOpcode.RightRotateH9Bit: Registers.H = RightRotate(Registers.H); break;
                case BitOpcode.RightRotateL9Bit: Registers.L = RightRotate(Registers.L); break;
                case BitOpcode.RightRotateAddressInHL9Bit: Memory.WriteByte(Registers.HL, RightRotate(Memory.ReadByte(Registers.HL))); break;
                case BitOpcode.RightRotateA9Bit: Registers.A = RightRotate(Registers.A); break;

                // 0x20
                case BitOpcode.LeftShiftBFill0: Registers.B = LeftShift(Registers.B); break;
                case BitOpcode.LeftShiftCFill0: Registers.C = LeftShift(Registers.C); break;
                case BitOpcode.LeftShiftDFill0: Registers.D = LeftShift(Registers.D); break;
                case BitOpcode.LeftShiftEFill0: Registers.E = LeftShift(Registers.E); break;
                case BitOpcode.LeftShiftHFill0: Registers.H = LeftShift(Registers.H); break;
                case BitOpcode.LeftShiftLFill0: Registers.L = LeftShift(Registers.L); break;
                case BitOpcode.LeftShiftAddressInHLFill0: Memory.WriteByte(Registers.HL, LeftShift(Memory.ReadByte(Registers.HL))); break;
                case BitOpcode.LeftShiftAFill0: Registers.A = LeftShift(Registers.A); break;
                case BitOpcode.RightShiftB: Registers.B = RightShift(Registers.B); break;
                case BitOpcode.RightShiftC: Registers.C = RightShift(Registers.C); break;
                case BitOpcode.RightShiftD: Registers.D = RightShift(Registers.D); break;
                case BitOpcode.RightShiftE: Registers.E = RightShift(Registers.E); break;
                case BitOpcode.RightShiftH: Registers.H = RightShift(Registers.H); break;
                case BitOpcode.RightShiftL: Registers.L = RightShift(Registers.L); break;
                case BitOpcode.RightShiftAddressInHL: Memory.WriteByte(Registers.HL, RightShift(Memory.ReadByte(Registers.HL))); break;
                case BitOpcode.RightShiftA: Registers.A = RightShift(Registers.A); break;

                // 0x30
                case BitOpcode.LeftShiftBFill1: Registers.B = LeftShiftFill1(Registers.B); break;
                case BitOpcode.LeftShiftCFill1: Registers.C = LeftShiftFill1(Registers.C); break;
                case BitOpcode.LeftShiftDFill1: Registers.D = LeftShiftFill1(Registers.D); break;
                case BitOpcode.LeftShiftEFill1: Registers.E = LeftShiftFill1(Registers.E); break;
                case BitOpcode.LeftShiftHFill1: Registers.H = LeftShiftFill1(Registers.H); break;
                case BitOpcode.LeftShiftLFill1: Registers.L = LeftShiftFill1(Registers.L); break;
                case BitOpcode.LeftShiftAddressInHLFill1: Memory.WriteByte(Registers.HL, LeftShiftFill1(Memory.ReadByte(Registers.HL))); break;
                case BitOpcode.LeftShiftAFill1: Registers.A = LeftShiftFill1(Registers.A); break;
                case BitOpcode.RightShiftBFill0: Registers.B = RightShiftFill0(Registers.B); break;
                case BitOpcode.RightShiftCFill0: Registers.C = RightShiftFill0(Registers.C); break;
                case BitOpcode.RightShiftDFill0: Registers.D = RightShiftFill0(Registers.D); break;
                case BitOpcode.RightShiftEFill0: Registers.E = RightShiftFill0(Registers.E); break;
                case BitOpcode.RightShiftHFill0: Registers.H = RightShiftFill0(Registers.H); break;
                case BitOpcode.RightShiftLFill0: Registers.L = RightShiftFill0(Registers.L); break;
                case BitOpcode.RightShiftAddressInHLFill0: Memory.WriteByte(Registers.HL, RightShiftFill0(Memory.ReadByte(Registers.HL))); break;
                case BitOpcode.RightShiftAFill0: Registers.A = RightShiftFill0(Registers.A); break;

                // 0x40
                case BitOpcode.TestBBit0: TestBit(Registers.B, 0); break;
                case BitOpcode.TestCBit0: TestBit(Registers.C, 0); break;
                case BitOpcode.TestDBit0: TestBit(Registers.D, 0); break;
                case BitOpcode.TestEBit0: TestBit(Registers.E, 0); break;
                case BitOpcode.TestHBit0: TestBit(Registers.H, 0); break;
                case BitOpcode.TestLBit0: TestBit(Registers.L, 0); break;
                case BitOpcode.TestAddressInHLBit0: TestBit(Memory.ReadByte(Registers.HL), 0); break;
                case BitOpcode.TestABit0: TestBit(Registers.A, 0); break;
                case BitOpcode.TestBBit1: TestBit(Registers.B, 1); break;
                case BitOpcode.TestCBit1: TestBit(Registers.C, 1); break;
                case BitOpcode.TestDBit1: TestBit(Registers.D, 1); break;
                case BitOpcode.TestEBit1: TestBit(Registers.E, 1); break;
                case BitOpcode.TestHBit1: TestBit(Registers.H, 1); break;
                case BitOpcode.TestLBit1: TestBit(Registers.L, 1); break;
                case BitOpcode.TestAddressInHLBit1: TestBit(Memory.ReadByte(Registers.HL), 1); break;
                case BitOpcode.TestABit1: TestBit(Registers.A, 1); break;

                // 0x50
                case BitOpcode.TestBBit2: TestBit(Registers.B, 2); break;
                case BitOpcode.TestCBit2: TestBit(Registers.C, 2); break;
                case BitOpcode.TestDBit2: TestBit(Registers.D, 2); break;
                case BitOpcode.TestEBit2: TestBit(Registers.E, 2); break;
                case BitOpcode.TestHBit2: TestBit(Registers.H, 2); break;
                case BitOpcode.TestLBit2: TestBit(Registers.L, 2); break;
                case BitOpcode.TestAddressInHLBit2: TestBit(Memory.ReadByte(Registers.HL), 2); break;
                case BitOpcode.TestABit2: TestBit(Registers.A, 2); break;
                case BitOpcode.TestBBit3: TestBit(Registers.B, 3); break;
                case BitOpcode.TestCBit3: TestBit(Registers.C, 3); break;
                case BitOpcode.TestDBit3: TestBit(Registers.D, 3); break;
                case BitOpcode.TestEBit3: TestBit(Registers.E, 3); break;
                case BitOpcode.TestHBit3: TestBit(Registers.H, 3); break;
                case BitOpcode.TestLBit3: TestBit(Registers.L, 3); break;
                case BitOpcode.TestAddressInHLBit3: TestBit(Memory.ReadByte(Registers.HL), 3); break;
                case BitOpcode.TestABit3: TestBit(Registers.A, 3); break;

                // 0x60
                case BitOpcode.TestBBit4: TestBit(Registers.B, 4); break;
                case BitOpcode.TestCBit4: TestBit(Registers.C, 4); break;
                case BitOpcode.TestDBit4: TestBit(Registers.D, 4); break;
                case BitOpcode.TestEBit4: TestBit(Registers.E, 4); break;
                case BitOpcode.TestHBit4: TestBit(Registers.H, 4); break;
                case BitOpcode.TestLBit4: TestBit(Registers.L, 4); break;
                case BitOpcode.TestAddressInHLBit4: TestBit(Memory.ReadByte(Registers.HL), 4); break;
                case BitOpcode.TestABit4: TestBit(Registers.A, 4); break;
                case BitOpcode.TestBBit5: TestBit(Registers.B, 5); break;
                case BitOpcode.TestCBit5: TestBit(Registers.C, 5); break;
                case BitOpcode.TestDBit5: TestBit(Registers.D, 5); break;
                case BitOpcode.TestEBit5: TestBit(Registers.E, 5); break;
                case BitOpcode.TestHBit5: TestBit(Registers.H, 5); break;
                case BitOpcode.TestLBit5: TestBit(Registers.L, 5); break;
                case BitOpcode.TestAddressInHLBit5: TestBit(Memory.ReadByte(Registers.HL), 5); break;
                case BitOpcode.TestABit5: TestBit(Registers.A, 5); break;

                // 0x70
                case BitOpcode.TestBBit6: TestBit(Registers.B, 6); break;
                case BitOpcode.TestCBit6: TestBit(Registers.C, 6); break;
                case BitOpcode.TestDBit6: TestBit(Registers.D, 6); break;
                case BitOpcode.TestEBit6: TestBit(Registers.E, 6); break;
                case BitOpcode.TestHBit6: TestBit(Registers.H, 6); break;
                case BitOpcode.TestLBit6: TestBit(Registers.L, 6); break;
                case BitOpcode.TestAddressInHLBit6: TestBit(Memory.ReadByte(Registers.HL), 6); break;
                case BitOpcode.TestABit6: TestBit(Registers.A, 6); break;
                case BitOpcode.TestBBit7: TestBit(Registers.B, 7); break;
                case BitOpcode.TestCBit7: TestBit(Registers.C, 7); break;
                case BitOpcode.TestDBit7: TestBit(Registers.D, 7); break;
                case BitOpcode.TestEBit7: TestBit(Registers.E, 7); break;
                case BitOpcode.TestHBit7: TestBit(Registers.H, 7); break;
                case BitOpcode.TestLBit7: TestBit(Registers.L, 7); break;
                case BitOpcode.TestAddressInHLBit7: TestBit(Memory.ReadByte(Registers.HL), 7); break;
                case BitOpcode.TestABit7: TestBit(Registers.A, 7); break;

                // 0x80
                case BitOpcode.ResetBBit0: Registers.B = ResetBit(Registers.B, 0); break;
                case BitOpcode.ResetCBit0: Registers.C = ResetBit(Registers.C, 0); break;
                case BitOpcode.ResetDBit0: Registers.D = ResetBit(Registers.D, 0); break;
                case BitOpcode.ResetEBit0: Registers.E = ResetBit(Registers.E, 0); break;
                case BitOpcode.ResetHBit0: Registers.H = ResetBit(Registers.H, 0); break;
                case BitOpcode.ResetLBit0: Registers.L = ResetBit(Registers.L, 0); break;
                case BitOpcode.ResetAddressInHLBit0: Memory.WriteByte(Registers.HL, ResetBit(Memory.ReadByte(Registers.HL), 0)); break;
                case BitOpcode.ResetABit0: Registers.A = ResetBit(Registers.A, 0); break;
                case BitOpcode.ResetBBit1: Registers.B = ResetBit(Registers.B, 1); break;
                case BitOpcode.ResetCBit1: Registers.C = ResetBit(Registers.C, 1); break;
                case BitOpcode.ResetDBit1: Registers.D = ResetBit(Registers.D, 1); break;
                case BitOpcode.ResetEBit1: Registers.E = ResetBit(Registers.E, 1); break;
                case BitOpcode.ResetHBit1: Registers.H = ResetBit(Registers.H, 1); break;
                case BitOpcode.ResetLBit1: Registers.L = ResetBit(Registers.L, 1); break;
                case BitOpcode.ResetAddressInHLBit1: Memory.WriteByte(Registers.HL, ResetBit(Memory.ReadByte(Registers.HL), 1)); break;
                case BitOpcode.ResetABit1: Registers.A = ResetBit(Registers.A, 1); break;
                               
                // 0x90        
                case BitOpcode.ResetBBit2: Registers.B = ResetBit(Registers.B, 2); break;
                case BitOpcode.ResetCBit2: Registers.C = ResetBit(Registers.C, 2); break;
                case BitOpcode.ResetDBit2: Registers.D = ResetBit(Registers.D, 2); break;
                case BitOpcode.ResetEBit2: Registers.E = ResetBit(Registers.E, 2); break;
                case BitOpcode.ResetHBit2: Registers.H = ResetBit(Registers.H, 2); break;
                case BitOpcode.ResetLBit2: Registers.L = ResetBit(Registers.L, 2); break;
                case BitOpcode.ResetAddressInHLBit2: Memory.WriteByte(Registers.HL, ResetBit(Memory.ReadByte(Registers.HL), 2)); break;
                case BitOpcode.ResetABit2: Registers.A = ResetBit(Registers.A, 2); break;
                case BitOpcode.ResetBBit3: Registers.B = ResetBit(Registers.B, 3); break;
                case BitOpcode.ResetCBit3: Registers.C = ResetBit(Registers.C, 3); break;
                case BitOpcode.ResetDBit3: Registers.D = ResetBit(Registers.D, 3); break;
                case BitOpcode.ResetEBit3: Registers.E = ResetBit(Registers.E, 3); break;
                case BitOpcode.ResetHBit3: Registers.H = ResetBit(Registers.H, 3); break;
                case BitOpcode.ResetLBit3: Registers.L = ResetBit(Registers.L, 3); break;
                case BitOpcode.ResetAddressInHLBit3: Memory.WriteByte(Registers.HL, ResetBit(Memory.ReadByte(Registers.HL), 3)); break;
                case BitOpcode.ResetABit3: Registers.A = ResetBit(Registers.A, 3); break;
                               
                // 0xA0        
                case BitOpcode.ResetBBit4: Registers.B = ResetBit(Registers.B, 4); break;
                case BitOpcode.ResetCBit4: Registers.C = ResetBit(Registers.C, 4); break;
                case BitOpcode.ResetDBit4: Registers.D = ResetBit(Registers.D, 4); break;
                case BitOpcode.ResetEBit4: Registers.E = ResetBit(Registers.E, 4); break;
                case BitOpcode.ResetHBit4: Registers.H = ResetBit(Registers.H, 4); break;
                case BitOpcode.ResetLBit4: Registers.L = ResetBit(Registers.L, 4); break;
                case BitOpcode.ResetAddressInHLBit4: Memory.WriteByte(Registers.HL, ResetBit(Memory.ReadByte(Registers.HL), 4)); break;
                case BitOpcode.ResetABit4: Registers.A = ResetBit(Registers.A, 4); break;
                case BitOpcode.ResetBBit5: Registers.B = ResetBit(Registers.B, 5); break;
                case BitOpcode.ResetCBit5: Registers.C = ResetBit(Registers.C, 5); break;
                case BitOpcode.ResetDBit5: Registers.D = ResetBit(Registers.D, 5); break;
                case BitOpcode.ResetEBit5: Registers.E = ResetBit(Registers.E, 5); break;
                case BitOpcode.ResetHBit5: Registers.H = ResetBit(Registers.H, 5); break;
                case BitOpcode.ResetLBit5: Registers.L = ResetBit(Registers.L, 5); break;
                case BitOpcode.ResetAddressInHLBit5: Memory.WriteByte(Registers.HL, ResetBit(Memory.ReadByte(Registers.HL), 5)); break;
                case BitOpcode.ResetABit5: Registers.A = ResetBit(Registers.A, 5); break;
                               
                // 0xB0        
                case BitOpcode.ResetBBit6: Registers.B = ResetBit(Registers.B, 6); break;
                case BitOpcode.ResetCBit6: Registers.C = ResetBit(Registers.C, 6); break;
                case BitOpcode.ResetDBit6: Registers.D = ResetBit(Registers.D, 6); break;
                case BitOpcode.ResetEBit6: Registers.E = ResetBit(Registers.E, 6); break;
                case BitOpcode.ResetHBit6: Registers.H = ResetBit(Registers.H, 6); break;
                case BitOpcode.ResetLBit6: Registers.L = ResetBit(Registers.L, 6); break;
                case BitOpcode.ResetAddressInHLBit6: Memory.WriteByte(Registers.HL, ResetBit(Memory.ReadByte(Registers.HL), 6)); break;
                case BitOpcode.ResetABit6: Registers.A = ResetBit(Registers.A, 6); break;
                case BitOpcode.ResetBBit7: Registers.B = ResetBit(Registers.B, 7); break;
                case BitOpcode.ResetCBit7: Registers.C = ResetBit(Registers.C, 7); break;
                case BitOpcode.ResetDBit7: Registers.D = ResetBit(Registers.D, 7); break;
                case BitOpcode.ResetEBit7: Registers.E = ResetBit(Registers.E, 7); break;
                case BitOpcode.ResetHBit7: Registers.H = ResetBit(Registers.H, 7); break;
                case BitOpcode.ResetLBit7: Registers.L = ResetBit(Registers.L, 7); break;
                case BitOpcode.ResetAddressInHLBit7: Memory.WriteByte(Registers.HL, ResetBit(Memory.ReadByte(Registers.HL), 7)); break;
                case BitOpcode.ResetABit7: Registers.A = ResetBit(Registers.A, 7); break;

                // 0xC0
                case BitOpcode.SetBBit0: Registers.B = SetBit(Registers.B, 0); break;
                case BitOpcode.SetCBit0: Registers.C = SetBit(Registers.C, 0); break;
                case BitOpcode.SetDBit0: Registers.D = SetBit(Registers.D, 0); break;
                case BitOpcode.SetEBit0: Registers.E = SetBit(Registers.E, 0); break;
                case BitOpcode.SetHBit0: Registers.H = SetBit(Registers.H, 0); break;
                case BitOpcode.SetLBit0: Registers.L = SetBit(Registers.L, 0); break;
                case BitOpcode.SetAddressInHLBit0: Memory.WriteByte(Registers.HL, SetBit(Memory.ReadByte(Registers.HL), 0)); break;
                case BitOpcode.SetABit0: Registers.A = SetBit(Registers.A, 0); break;
                case BitOpcode.SetBBit1: Registers.B = SetBit(Registers.B, 1); break;
                case BitOpcode.SetCBit1: Registers.C = SetBit(Registers.C, 1); break;
                case BitOpcode.SetDBit1: Registers.D = SetBit(Registers.D, 1); break;
                case BitOpcode.SetEBit1: Registers.E = SetBit(Registers.E, 1); break;
                case BitOpcode.SetHBit1: Registers.H = SetBit(Registers.H, 1); break;
                case BitOpcode.SetLBit1: Registers.L = SetBit(Registers.L, 1); break;
                case BitOpcode.SetAddressInHLBit1: Memory.WriteByte(Registers.HL, SetBit(Memory.ReadByte(Registers.HL), 1)); break;
                case BitOpcode.SetABit1: Registers.A = SetBit(Registers.A, 1); break;
                               
                // 0xD0        
                case BitOpcode.SetBBit2: Registers.B = SetBit(Registers.B, 2); break;
                case BitOpcode.SetCBit2: Registers.C = SetBit(Registers.C, 2); break;
                case BitOpcode.SetDBit2: Registers.D = SetBit(Registers.D, 2); break;
                case BitOpcode.SetEBit2: Registers.E = SetBit(Registers.E, 2); break;
                case BitOpcode.SetHBit2: Registers.H = SetBit(Registers.H, 2); break;
                case BitOpcode.SetLBit2: Registers.L = SetBit(Registers.L, 2); break;
                case BitOpcode.SetAddressInHLBit2: Memory.WriteByte(Registers.HL, SetBit(Memory.ReadByte(Registers.HL), 2)); break;
                case BitOpcode.SetABit2: Registers.A = SetBit(Registers.A, 2); break;
                case BitOpcode.SetBBit3: Registers.B = SetBit(Registers.B, 3); break;
                case BitOpcode.SetCBit3: Registers.C = SetBit(Registers.C, 3); break;
                case BitOpcode.SetDBit3: Registers.D = SetBit(Registers.D, 3); break;
                case BitOpcode.SetEBit3: Registers.E = SetBit(Registers.E, 3); break;
                case BitOpcode.SetHBit3: Registers.H = SetBit(Registers.H, 3); break;
                case BitOpcode.SetLBit3: Registers.L = SetBit(Registers.L, 3); break;
                case BitOpcode.SetAddressInHLBit3: Memory.WriteByte(Registers.HL, SetBit(Memory.ReadByte(Registers.HL), 3)); break;
                case BitOpcode.SetABit3: Registers.A = SetBit(Registers.A, 3); break;
                               
                // 0xE0        
                case BitOpcode.SetBBit4: Registers.B = SetBit(Registers.B, 4); break;
                case BitOpcode.SetCBit4: Registers.C = SetBit(Registers.C, 4); break;
                case BitOpcode.SetDBit4: Registers.D = SetBit(Registers.D, 4); break;
                case BitOpcode.SetEBit4: Registers.E = SetBit(Registers.E, 4); break;
                case BitOpcode.SetHBit4: Registers.H = SetBit(Registers.H, 4); break;
                case BitOpcode.SetLBit4: Registers.L = SetBit(Registers.L, 4); break;
                case BitOpcode.SetAddressInHLBit4: Memory.WriteByte(Registers.HL, SetBit(Memory.ReadByte(Registers.HL), 4)); break;
                case BitOpcode.SetABit4: Registers.A = SetBit(Registers.A, 4); break;
                case BitOpcode.SetBBit5: Registers.B = SetBit(Registers.B, 5); break;
                case BitOpcode.SetCBit5: Registers.C = SetBit(Registers.C, 5); break;
                case BitOpcode.SetDBit5: Registers.D = SetBit(Registers.D, 5); break;
                case BitOpcode.SetEBit5: Registers.E = SetBit(Registers.E, 5); break;
                case BitOpcode.SetHBit5: Registers.H = SetBit(Registers.H, 5); break;
                case BitOpcode.SetLBit5: Registers.L = SetBit(Registers.L, 5); break;
                case BitOpcode.SetAddressInHLBit5: Memory.WriteByte(Registers.HL, SetBit(Memory.ReadByte(Registers.HL), 5)); break;
                case BitOpcode.SetABit5: Registers.A = SetBit(Registers.A, 5); break;
                               
                // 0xF0        
                case BitOpcode.SetBBit6: Registers.B = SetBit(Registers.B, 6); break;
                case BitOpcode.SetCBit6: Registers.C = SetBit(Registers.C, 6); break;
                case BitOpcode.SetDBit6: Registers.D = SetBit(Registers.D, 6); break;
                case BitOpcode.SetEBit6: Registers.E = SetBit(Registers.E, 6); break;
                case BitOpcode.SetHBit6: Registers.H = SetBit(Registers.H, 6); break;
                case BitOpcode.SetLBit6: Registers.L = SetBit(Registers.L, 6); break;
                case BitOpcode.SetAddressInHLBit6: Memory.WriteByte(Registers.HL, SetBit(Memory.ReadByte(Registers.HL), 6)); break;
                case BitOpcode.SetABit6: Registers.A = SetBit(Registers.A, 6); break;
                case BitOpcode.SetBBit7: Registers.B = SetBit(Registers.B, 7); break;
                case BitOpcode.SetCBit7: Registers.C = SetBit(Registers.C, 7); break;
                case BitOpcode.SetDBit7: Registers.D = SetBit(Registers.D, 7); break;
                case BitOpcode.SetEBit7: Registers.E = SetBit(Registers.E, 7); break;
                case BitOpcode.SetHBit7: Registers.H = SetBit(Registers.H, 7); break;
                case BitOpcode.SetLBit7: Registers.L = SetBit(Registers.L, 7); break;
                case BitOpcode.SetAddressInHLBit7: Memory.WriteByte(Registers.HL, SetBit(Memory.ReadByte(Registers.HL), 7)); break;
                case BitOpcode.SetABit7: Registers.A = SetBit(Registers.A, 7); break;
            }
        }

        private byte LeftShift(byte value)
        {
            byte newValue = (byte)(value << 1);

            SetStateFlags(newValue);
            Registers.Flags.AddSubtract = false;
            Registers.Flags.HalfCarry = false;
            Registers.Flags.Carry = (value >> 7) != 0;
            Registers.Flags.Parity = DetectParity(newValue);

            return newValue;
        }

        private byte RightShift(byte value)
        {
            byte newValue = (byte)((value >> 1) | (value & (1 << 7)));

            SetStateFlags(newValue);
            Registers.Flags.AddSubtract = false;
            Registers.Flags.HalfCarry = false;
            Registers.Flags.Carry = (value & 1) != 0;
            Registers.Flags.Parity = DetectParity(newValue);

            return newValue;
        }

        private byte LeftShiftFill1(byte value)
        {
            byte newValue = (byte)((value << 1) | 1);

            SetStateFlags(newValue);
            Registers.Flags.AddSubtract = false;
            Registers.Flags.HalfCarry = false;
            Registers.Flags.Carry = (value >> 7) != 0;
            Registers.Flags.Parity = DetectParity(newValue);

            return newValue;
        }

        private byte RightShiftFill0(byte value)
        {
            byte newValue = (byte)((value >> 1) & (value & ~(1 << 7)));

            SetStateFlags(newValue);
            Registers.Flags.AddSubtract = false;
            Registers.Flags.HalfCarry = false;
            Registers.Flags.Carry = (value & 1) != 0;
            Registers.Flags.Parity = DetectParity(newValue);

            return newValue;
        }

        private void TestBit(byte value, int bit)
        {
            bool bitValue = ((value >> bit) & 1) != 0;

            Registers.Flags.Zero = !bitValue;
            Registers.Flags.HalfCarry = true;
            Registers.Flags.AddSubtract = false;
        }

        private byte ResetBit(byte value, int bit)
        {
            return (byte)(value & ~(1 << bit));
        }

        private byte SetBit(byte value, int bit)
        {
            return (byte)(value | (1 << bit));
        }


        #endregion

        #region Misc Instructions
        private void MiscInstruction()
        {
            byte opcode = ReadNextByte();

            switch (opcode)
            {
                // 0x40
                case MiscOpcode.InputFromPortCToB: Registers.B = Ports.Read(Registers.C); break;
                case MiscOpcode.OutputBToPortC: Ports.Write(Registers.C, Registers.B); break;
                case MiscOpcode.SubCarryBCFromHL: Registers.HL = SubWithCarry(Registers.HL, Registers.BC); break;
                case MiscOpcode.StoreBCInImmediateAddress: WriteWord(ReadNextWord(), Registers.BC); break;
                case MiscOpcode.NegateA: Registers.A = Sub(0, Registers.A); break;
                case MiscOpcode.ReturnNonMaskableInterrupt: ReturnFromNonMaskableInterrupt(); break;
                case MiscOpcode.SetInterruptMode0: _interruptHandler.InterruptMode = InterruptMode.Zero; break;
                case MiscOpcode.StoreAInInterruptVector: Registers.InterruptVector = Registers.A; break;
                case MiscOpcode.InputFromPortCToC: Registers.C = Ports.Read(Registers.C); break;
                case MiscOpcode.OutputCToPortC: Ports.Write(Registers.C, Registers.C); break;
                case MiscOpcode.AddCarryBCToHL: Registers.HL = AddWithCarry(Registers.HL, Registers.BC); break;
                case MiscOpcode.LoadBCFromImmediateAddress: Registers.BC = ReadNextWord(); break;
                case MiscOpcode.ReturnMaskableInterrupt: ReturnFromMaskableInterrupt(); break;
                case MiscOpcode.StoreAInRefreshCounter: Registers.RefreshCounter = Registers.A; break;

                // 0x50
                case MiscOpcode.InputFromPortCToD: Registers.D = Ports.Read(Registers.C); break;
                case MiscOpcode.OutputDToPortC: Ports.Write(Registers.C, Registers.D); break;
                case MiscOpcode.SubCarryDEFromHL: Registers.HL = SubWithCarry(Registers.HL, Registers.DE); break;
                case MiscOpcode.StoreDEInImmediateAddress: WriteWord(ReadNextWord(), Registers.DE); break;
                case MiscOpcode.SetInterruptMode1: _interruptHandler.InterruptMode = InterruptMode.One; break;
                case MiscOpcode.LoadInterruptVectorToA: Registers.A = Registers.InterruptVector; break;
                case MiscOpcode.InputFromPortCToE: Registers.E = Ports.Read(Registers.C); break;
                case MiscOpcode.OutputEToPortC: Ports.Write(Registers.C, Registers.E); break;
                case MiscOpcode.AddCarryDEToHL: Registers.HL = AddWithCarry(Registers.HL, Registers.DE); break;
                case MiscOpcode.LoadDEFromImmediateAddress: Registers.DE = ReadNextWord(); break;
                case MiscOpcode.SetInterruptMode2: _interruptHandler.InterruptMode = InterruptMode.Two; break;
                case MiscOpcode.LoadRefreshCounterToA: Registers.A = Registers.RefreshCounter; break;

                // 0x60
                case MiscOpcode.InputFromPortCToH: Registers.H = Ports.Read(Registers.C); break;
                case MiscOpcode.OutputHToPortC: Ports.Write(Registers.C, Registers.H); break;
                case MiscOpcode.SubCarryHLFromHL: Registers.HL = SubWithCarry(Registers.HL, Registers.HL); break;
                case MiscOpcode.StoreHLInImmediateAddress: WriteWord(ReadNextWord(), Registers.HL); break;
                case MiscOpcode.RotateRightDecimal: RightRotateDecimal(); break;
                case MiscOpcode.InputFromPortCToL: Registers.L = Ports.Read(Registers.C); break;
                case MiscOpcode.OutputAToPortL: Ports.Write(Registers.C, Registers.L); break;
                case MiscOpcode.AddCarryHLToHL: Registers.HL = AddWithCarry(Registers.HL, Registers.HL); break;
                case MiscOpcode.LoadHLFromImmediateAddress: Registers.HL = ReadNextWord(); break;
                case MiscOpcode.LeftRotateDecimal: LeftRotateDecimal(); break;

                // 0x70
                case MiscOpcode.InputFromCFlagsOnly: Ports.Read(Registers.C); break;
                case MiscOpcode.Output0ToPortC: Ports.Write(Registers.C, 0); break;
                case MiscOpcode.SubCarryStackPointerFromHL: Registers.HL = SubWithCarry(Registers.HL, Registers.StackPointer); break;
                case MiscOpcode.StoreStackPointerInImmediateAddress: WriteWord(ReadNextWord(), Registers.StackPointer); break;
                case MiscOpcode.InputFromPortCToA: Registers.A = Ports.Read(Registers.C); break;
                case MiscOpcode.OutputAToPortC: Ports.Write(Registers.C, Registers.A); break;
                case MiscOpcode.AddCarryStackPointerToHL: Registers.HL = AddWithCarry(Registers.HL, Registers.StackPointer); break;
                case MiscOpcode.LoadSPFromImmediateAddress: Registers.StackPointer = ReadNextWord(); break;

                // 0xA0
                case MiscOpcode.MoveIncrement: MoveIncrement(); break;
                case MiscOpcode.CompareIncrement: CompareIncrement(); break;
                case MiscOpcode.InputIncrement: InputIncrement(); break;
                case MiscOpcode.OutputIncrement: OutputIncrement(); break;
                case MiscOpcode.MoveDecrement: MoveDecrement(); break;
                case MiscOpcode.CompareDecrement: CompareDecrement(); break;
                case MiscOpcode.InputDecrement: InputDecrement(); break;
                case MiscOpcode.OutputDecrement: OutputDecrement(); break;

                // 0xB0
                case MiscOpcode.MoveIncrementRepeat: MoveIncrementRepeat(); break;
                case MiscOpcode.CompareIncrementRepeat: CompareIncrementRepeat(); break;
                case MiscOpcode.InputIncrementRepeat: InputIncrementRepeat(); break;
                case MiscOpcode.OutputIncrementRepeat: OutputIncrementRepeat(); break;
                case MiscOpcode.MoveDecrementRepeat: MoveDecrementRepeat(); break;
                case MiscOpcode.CompareDecrementRepeat: CompareDecrementRepeat(); break;
                case MiscOpcode.InputDecrementRepeat: InputDecrementRepeat(); break;
                case MiscOpcode.OutputDecrementRepeat: OutputDecrementRepeat(); break;

                default: Console.WriteLine("Invalid Misc Instruction: ED " + opcode); break;

            }
        }

        private void MoveIncrement()
        {
            Memory.WriteByte(Registers.DE, Memory.ReadByte(Registers.HL));
            Registers.DE++;
            Registers.HL++;
            Registers.BC--;
            Registers.Flags.AddSubtract = false;
            Registers.Flags.HalfCarry = false;
            Registers.Flags.Overflow = Registers.BC == 0;
        }

        private void CompareIncrement()
        {
            Sub(Memory.ReadByte(Registers.HL), Registers.A);
            Registers.DE++;
            Registers.HL++;
            Registers.BC--;
            Registers.Flags.AddSubtract = true;
            Registers.Flags.Overflow = Registers.BC == 0;
        }

        private void InputIncrement()
        {
            Memory.WriteByte(Registers.HL, Ports.Read(Registers.C));
            Registers.DE++;
            Registers.HL++;
            Registers.BC--;
            Registers.Flags.AddSubtract = true;
            Registers.Flags.Overflow = Registers.BC == 0;
        }

        private void OutputIncrement()
        {
            Ports.Write(Registers.C, Memory.ReadByte(Registers.HL));
            Registers.DE++;
            Registers.HL++;
            Registers.BC--;
            Registers.Flags.AddSubtract = true;
            Registers.Flags.Overflow = Registers.BC == 0;
        }
        
        private void MoveDecrement()
        {
            Memory.WriteByte(Registers.DE, Memory.ReadByte(Registers.HL));
            Registers.DE--;
            Registers.HL--;
            Registers.BC--;
            Registers.Flags.AddSubtract = false;
            Registers.Flags.HalfCarry = false;
            Registers.Flags.Overflow = Registers.BC == 0;
        }

        private void CompareDecrement()
        {
            Sub(Memory.ReadByte(Registers.HL), Registers.A);
            Registers.DE--;
            Registers.HL--;
            Registers.BC--;
            Registers.Flags.AddSubtract = true;
            Registers.Flags.Overflow = Registers.BC == 0;
        }

        private void InputDecrement()
        {
            Memory.WriteByte(Registers.HL, Ports.Read(Registers.C));
            Registers.DE--;
            Registers.HL--;
            Registers.BC--;
            Registers.Flags.AddSubtract = true;
            Registers.Flags.Overflow = Registers.BC == 0;
        }

        private void OutputDecrement()
        {
            Ports.Write(Registers.C, Memory.ReadByte(Registers.HL));
            Registers.DE--;
            Registers.HL--;
            Registers.BC--;
            Registers.Flags.AddSubtract = true;
            Registers.Flags.Overflow = Registers.BC == 0;
        }
        #endregion

        #region IX Instructions
        private void IXInstruction()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IYInstructions
        private void IYInstruction()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Exchange Helpers
        private void ExchangeStackTopWithHL()
        {
            byte upper = Memory.ReadByte(Registers.StackPointer);
            byte lower = Memory.ReadByte(Registers.StackPointer + 1);
            ushort temp = Registers.HL;
            Registers.HL = (ushort)(upper << 8 | lower);
            Memory.WriteByte(Registers.StackPointer, (byte)(temp >> 8));
            Memory.WriteByte(Registers.StackPointer + 1, (byte)temp);
        }
        #endregion

        #region Read/Write Data
        private byte ReadNextByte()
        {
            byte value = Memory.ReadByte(Registers.ProgramCounter);
            Registers.ProgramCounter++;
            return value;
        }

        private ushort ReadNextWord()
        {
            byte lower = ReadNextByte();
            byte upper = ReadNextByte();
            return (ushort)(upper << 8 | lower);
        }

        private void WriteWord(int address, ushort value)
        {
            Memory.WriteByte(address, (byte)value);
            Memory.WriteByte(address + 1, (byte)(value >> 8));
        }

        public ushort ReadWordFromAddress(ushort address)
        {
            byte lower = Memory.ReadByte(address);
            byte upper = Memory.ReadByte(address + 1);
            return (ushort)(upper << 8 | lower);
        }
        #endregion

        #region Math
        private ushort Increment(ushort value) => (ushort)(value + 1);
        private byte Increment(byte value)
        {
            int newValue = value + 1;
            Registers.MainRegisters.Flags.AddSubtract = false;
            Registers.MainRegisters.Flags.Overflow = newValue > byte.MaxValue;
            SetHalfCarryAdd(value, 1);
            SetStateFlags((byte)newValue);
            return (byte)newValue;
        }
        private ushort Decrement(ushort value) => (ushort)(value - 1);
        private byte Decrement(byte value)
        {
            int newValue = value + 1;
            Registers.MainRegisters.Flags.AddSubtract = true;
            Registers.MainRegisters.Flags.Overflow = value < 1;
            SetHalfCarrySub(value, 1);
            SetStateFlags((byte)newValue);
            return (byte)newValue;
        }

        private ushort SubWithCarry(ushort value, ushort sub)
        {
            throw new NotImplementedException();
        }

        private byte SubWithCarry(byte value, byte sub)
        {
            int newValue = value - sub - (Registers.Flags.Carry ? 1 : 0); // TODO check how overflow and carry work
            Registers.Flags.AddSubtract = true;
            Registers.Flags.Carry = newValue < 0;
            Registers.Flags.Overflow = Registers.Flags.Carry;
            Registers.Flags.Sign = (newValue & (1 << 7)) != 0;
            SetHalfCarrySub(value, (byte)(sub + (Registers.Flags.Carry ? 1 : 0)));

            return (byte)newValue;
        }
        private byte Sub(byte value, byte sub)
        {
            int newValue = value - sub;
            Registers.Flags.AddSubtract = true;
            Registers.Flags.Carry = newValue < 0;
            Registers.Flags.Overflow = Registers.Flags.Carry; // TODO check how overflow and carry work
            Registers.Flags.Zero = newValue == 0;
            Registers.Flags.Sign = (newValue & (1 << 7)) != 0;
            SetHalfCarrySub(value, sub);

            return (byte)newValue;
        }
        private byte AddWithCarry(byte value, byte add)
        {
            int newValue = value + add + (Registers.Flags.Carry ? 1 : 0);
            Registers.Flags.AddSubtract = false;
            Registers.Flags.Carry = newValue > byte.MaxValue;
            Registers.Flags.Overflow = Registers.Flags.Carry;
            Registers.Flags.Zero = newValue == 0;
            Registers.Flags.Sign = (newValue & (1 << 7)) != 0;
            SetHalfCarryAdd(value, add);

            return (byte)newValue;
        }

        private ushort AddWithCarry(ushort value, ushort add)
        {
            throw new NotImplementedException();
        }
        private byte Add(byte value, byte add)
        {
            int newValue = value + add;
            Registers.Flags.AddSubtract = false;
            Registers.Flags.Carry = newValue > byte.MaxValue;
            Registers.Flags.Overflow = Registers.Flags.Carry;
            Registers.Flags.Zero = newValue == 0;
            Registers.Flags.Sign = (newValue & (1 << 7)) != 0;
            SetHalfCarryAdd(value, add);

            return (byte)newValue;
        }
        private ushort Add(ushort value, ushort add)
        {
            int newValue = value + add;
            Registers.Flags.AddSubtract = false;
            Registers.Flags.Carry = newValue > ushort.MaxValue;
            SetHalfCarryAdd((byte)value, (byte)add);

            return (ushort)newValue;
        }
        #endregion

        #region Bit Rotation
        private byte LeftRotate(byte value)
        {
            int newValue = value << 1;
            int overflowBit = (value & (1 << 8));
            Registers.Flags.Carry = overflowBit != 0;
            Registers.Flags.AddSubtract = false;
            Registers.Flags.HalfCarry = false;
            return (byte)(newValue & (overflowBit >> 7));
        }
        private byte RightRotate(byte value)
        {
            int newValue = value >> 1;
            int overflowBit = (value & 1);
            Registers.Flags.Carry = overflowBit != 0;
            Registers.Flags.AddSubtract = false;
            Registers.Flags.HalfCarry = false;
            return (byte)(newValue & overflowBit << 7);
        }
        private byte LeftRotate9Bit(byte value)
        {
            int newValue = value << 1 | (Registers.Flags.Carry ? 1 : 0);
            int overflowBit = (value & (1 << 8));
            Registers.Flags.Carry = overflowBit != 0;
            Registers.Flags.AddSubtract = false;
            Registers.Flags.HalfCarry = false;
            return (byte)newValue;
        }
        private byte RightRotate9Bit(byte value)
        {
            int newValue = value >> 1 | (Registers.Flags.Carry ? 1 << 7 : 0);
            int overflowBit = (value & 1);
            Registers.Flags.Carry = overflowBit != 0;
            Registers.Flags.AddSubtract = false;
            Registers.Flags.HalfCarry = false;
            return (byte)newValue;
        }
        #endregion

        #region Bitwise
        private byte And(byte value, byte and)
        {
            int newValue = value & and;
            Registers.Flags.Carry = false;
            Registers.Flags.AddSubtract = false;
            Registers.Flags.Parity = DetectParity((byte)newValue);
            Registers.Flags.HalfCarry = true;
            Registers.Flags.Zero = newValue == 0;
            Registers.Flags.Sign = (newValue & (1 << 7)) != 0;


            return (byte)newValue;
        }

        private byte Xor(byte value, byte xor)
        {
            int newValue = value ^ xor;
            Registers.Flags.Carry = false;
            Registers.Flags.AddSubtract = false;
            Registers.Flags.Parity = DetectParity((byte)newValue);
            Registers.Flags.HalfCarry = true;
            Registers.Flags.Zero = newValue == 0;
            Registers.Flags.Sign = (newValue & (1 << 7)) != 0;


            return (byte)newValue;
        }

        private byte Or(byte value, byte xor)
        {
            int newValue = value | xor;
            Registers.Flags.Carry = false;
            Registers.Flags.AddSubtract = false;
            Registers.Flags.Parity = DetectParity((byte)newValue);
            Registers.Flags.HalfCarry = true;
            Registers.Flags.Zero = newValue == 0;
            Registers.Flags.Sign = (newValue & (1 << 7)) != 0;


            return (byte)newValue;
        }

        private byte Invert(byte value)
        {
            Registers.Flags.AddSubtract = true;
            Registers.Flags.HalfCarry = true;
            return (byte)~value;
        }
        #endregion

        #region Flag Helpers
        private bool DetectParity(byte value)
        {
            BitArray bits = new BitArray(value);
            int count = 0;
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                {
                    count++;
                }
            }
            return (count & 1) != 0;
        }

        private void SetHalfCarrySub(byte value, byte sub)
        {
            Registers.MainRegisters.Flags.HalfCarry = (value & 0xF) < (sub & 0xF);
        }
        private void SetHalfCarryAdd(byte value, byte add)
        {
            Registers.MainRegisters.Flags.HalfCarry = (byte)(value & 0xF) + add > 0xF;
        }

        private void SetStateFlags(byte value)
        {
            Registers.MainRegisters.Flags.Zero = value == 0;
            Registers.MainRegisters.Flags.Sign = (value & (1 << 7)) != 0;
        }
        #endregion

        #region Jumps
        private void JumpRelativeIf(bool condition, sbyte offset)
        {
            ushort address = (ushort)(_instructionStart + offset);
            JumpAbsoluteIf(condition, address);
        }

        private void JumpAbsoluteIf(bool condition, ushort address)
        {
            if (condition)
            {
                Registers.ProgramCounter = address;
            }
        }

        private void Loop(sbyte offset)
        {
            Registers.B = (byte)(Registers.B - 1);
            JumpRelativeIf(Registers.B == 0, offset);
        }
        #endregion

        #region Stack Helpers
        private void Push(byte value)
        {
            Registers.StackPointer--;
            Memory.WriteByte(Registers.StackPointer, value);
        }
        private void Push(ushort value)
        {
            Push((byte)value);
            Push((byte)(value >> 8));
        }
        private byte PopByte()
        {
            byte value = Memory.ReadByte(Registers.StackPointer);
            Registers.StackPointer++;
            return value;
        }
        private ushort PopWord()
        {
            byte upper = PopByte();
            byte lower = PopByte();
            return (ushort)(upper << 8 | lower);
        }
        internal void Call(bool condition, ushort address)
        {
            if (condition)
            {
                Push(Registers.ProgramCounter);
                Registers.ProgramCounter = address;
            }
        }
        internal void Return(bool condition)
        {
            if (condition)
            {
                Registers.ProgramCounter = PopWord();
            }
        }
        #endregion

        #region Decimal
        private void LeftRotateDecimal()
        {
            byte temp = (byte)(Registers.A & 0x0F);
            Registers.A &= 0xF0;
            Registers.A |= (byte)(Memory.ReadByte(Registers.HL) >> 4);
            Memory.WriteByte(Registers.HL, (byte)((Memory.ReadByte(Registers.HL) << 4) | temp));


            Registers.Flags.AddSubtract = false;
            Registers.Flags.HalfCarry = false;
            SetStateFlags(Memory.ReadByte(Registers.HL));
            Registers.Flags.Parity = DetectParity(Memory.ReadByte(Registers.HL));
        }

        private void RightRotateDecimal()
        {
            byte temp = (byte)((Registers.A & 0x0F) << 4);
            Registers.A &= 0xF0;
            Registers.A |= (byte)(Memory.ReadByte(Registers.HL) & 0x0F);
            Memory.WriteByte(Registers.HL, (byte)((Memory.ReadByte(Registers.HL) >> 4) | temp));


            Registers.Flags.AddSubtract = false;
            Registers.Flags.HalfCarry = false;
            SetStateFlags(Memory.ReadByte(Registers.HL));
            Registers.Flags.Parity = DetectParity(Memory.ReadByte(Registers.HL));
        }
        #endregion
    }
}
