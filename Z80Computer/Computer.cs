using Z80CPUEmulator;

namespace Z80Computer
{
    public class Computer
    {
        private Z80 _cpu;


        public Computer(byte[] code)
        {
            Memory memory = new Memory();
            memory.WriteBytes(0, code);
            PortHandler portHandler = new PortHandler(memory);
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
