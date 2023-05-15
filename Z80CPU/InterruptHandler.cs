using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z80CPUEmulator
{
    public class InterruptHandler
    {
        private Z80 _cpu;
        private bool _interruptFlipFlop;
        private bool _interruptFlipFlop2;
        public bool InterruptsEnabled
        {
            set
            {
                _interruptFlipFlop = value;
                _interruptFlipFlop2 = value;
            }
        }
        public InterruptMode InterruptMode { get; set; } = InterruptMode.Zero;


        public InterruptHandler(Z80 cpu) {
            _cpu = cpu;
        }

        public void NonMaskableInterrupt()
        {
            _interruptFlipFlop2 = _interruptFlipFlop;
            _interruptFlipFlop = false;
            _cpu.Halted = false;
            _cpu.Call(true, 0x0066);
        }

        public void MaskableInterrupt(byte bus)
        {
            if (!_interruptFlipFlop)
            {
                return;
            }
            _interruptFlipFlop2 = _interruptFlipFlop;
            _interruptFlipFlop = false;
            _cpu.Halted = false;
            switch (InterruptMode)
            {
                case InterruptMode.Zero:
                    _cpu.ExecuteInstruction(bus);
                    break;
                case InterruptMode.One:
                    _cpu.Call(true, 0x0038);
                    break;
                case InterruptMode.Two:
                    ushort address = _cpu.ReadWordFromAddress((ushort)((_cpu.Registers.InterruptVector << 8) | bus & ~1));
                    _cpu.Call(true, address);
                    break;
            }
        }

        public void ReturnFromInterrupt()
        {
            _interruptFlipFlop = _interruptFlipFlop2;
            _cpu.Return(true);
        }
    }
    public enum InterruptMode
    {
        Zero, One, Two
    }
}
