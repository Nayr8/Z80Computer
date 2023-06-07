// See https://aka.ms/new-console-template for more information


using Z80Assembler.old;

class Program2
{
    void main()
    {
        const string path = "../../../../Z80Kernel";

        Assembler assembler = new Assembler();
        byte[] assembledCode = assembler.Assemble(path);

        if (assembler.Errors())
        {
            Console.WriteLine($"Failed at stage: {assembledCode[0]}");
            assembler.DumpErrors();
            assembler.DumpTokens();
            return;
        }

        string byteOrBytes = assembledCode.Length > 1 ? "bytes" : "byte";
        Console.WriteLine($"Writing {assembledCode.Length} {byteOrBytes} to AssembledZ80.bin");
        File.Delete("../../../../Z80Kernel/Build/Kernel.bin");
        FileStream output = File.Open("../../../../Z80Kernel/Build/Kernel.bin", FileMode.OpenOrCreate);
        output.Write(assembledCode);
        output.Flush();
    }
    
}
