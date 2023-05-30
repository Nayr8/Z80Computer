// See https://aka.ms/new-console-template for more information


using Z80Assembler;


string code = File.ReadAllText("../../../AssembleMe.z80");

Assembler assembler = new Assembler(code, 0x0000);
byte[] assembledCode = assembler.Assemble();

if (assembler.Errors())
{
    Console.WriteLine($"Failed at stage: {assembledCode[0]}");
    assembler.DumpErrors();
    assembler.DumpTokens();
    return;
}

string byteOrBytes = assembledCode.Length > 1 ? "bytes" : "byte";
Console.WriteLine($"Writing {assembledCode.Length} {byteOrBytes} to AssembledZ80.bin");
File.Delete("../../../AssembledZ80.bin");
FileStream output = File.Open("../../../AssembledZ80.bin", FileMode.OpenOrCreate);
output.Write(assembledCode);
output.Flush();