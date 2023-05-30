

using Z80Computer;

byte[] code = File.ReadAllBytes("../../../../Z80Assembler/AssembledZ80.bin");
Computer computer = new Computer(code);
computer.Run();
