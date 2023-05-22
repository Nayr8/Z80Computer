using Z80CPUEmulator;

namespace Z80Computer
{
    public class Computer
    {
        private Z80 _cpu;


        public Computer()
        {
            Memory memory = new Memory();
            PortHandler portHandler = new PortHandler(memory);
            _cpu = new Z80(memory, portHandler);
        }
    }
}
