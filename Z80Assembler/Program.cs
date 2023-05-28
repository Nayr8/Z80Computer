// See https://aka.ms/new-console-template for more information


using Z80Assembler;
/*
const string code = @"
@zero 0

start: ; The start label


; end comment";
*/


while (true)
{
    Console.Write("Enter line: ");
    string code = Console.ReadLine() ?? "";
    if (code is "exit") break;
    
    Assembler assembler = new(code);
    assembler.AddVariable("test", 0x56);
    assembler.AddVariable("boob", 0xB00B);
    byte[] binary = assembler.Assemble();
    
    Console.Write("Tokens: ");
    assembler.DumpTokens(); Console.WriteLine();

    if (assembler.Errors())
    {
        Console.Write("Errors: ");
        assembler.DumpErrors(); Console.WriteLine();
    }
    
    Console.Write("Code: ");
    BinaryLogger(binary); Console.WriteLine();
}

void BinaryLogger(byte[] binary)
{
    Console.WriteLine(binary.Length switch
    {
        0 => "No Code",
        1 => $"[0x{binary[0]:X2}]",
        2 => $"[0x{binary[0]:X2}, 0x{binary[1]:X2}]",
        3 => $"[0x{binary[0]:X2}, 0x{binary[1]:X2}, 0x{binary[2]:X2}]",
        4 => $"[0x{binary[0]:X2}, 0x{binary[1]:X2}, 0x{binary[2]:X2}, 0x{binary[3]:X2}]",
        _ => "Err"
    });
}