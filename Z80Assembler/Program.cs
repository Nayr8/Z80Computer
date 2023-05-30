// See https://aka.ms/new-console-template for more information


using Z80Assembler;
/*
const string code = @"
@zero 0

start: ; The start label


; end comment";
*/
string code = File.ReadAllText("../../../AssembleMe.z80");
Console.WriteLine(code);

Assembler assembler = new Assembler(code, 0x0000);
byte[] assembledCode = assembler.Assemble();

if (assembler.Errors())
{
    assembler.DumpErrors();
    return;
}

string byteOrBytes = assembledCode.Length > 0 ? "bytes" : "byte";
Console.WriteLine($"Writing {assembledCode.Length} {byteOrBytes} to AssembledZ80.bin");
File.Delete("../../../AssembledZ80.bin");
FileStream output = File.Open("../../../AssembledZ80.bin", FileMode.OpenOrCreate);
output.Write(assembledCode);
output.Flush();