using Z80CPUEmulator;

namespace Z80Computer
{
    public class Computer
    {
        private Z80 _cpu;

        public Computer(byte[] rom)
        {
            Memory memory = new();
            memory.WriteToRom(0, rom);
            PortHandler portHandler = new(memory);
            _cpu = new Z80(memory, portHandler);
        }

        public void Run()
        {
            while (!_cpu.Halted)
            {
                _cpu.ExecuteNextInstruction();
            }
        }
    }
}
