

using Z80Computer;

byte[] rom = File.ReadAllBytes("../../../../Z80Kernel/Build/Kernel.bin");
Computer computer = new(rom);
computer.Run();
