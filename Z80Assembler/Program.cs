

using Z80Assembler;

const string text = @"
main:
    ld a, 13
loop:
    dec a ; Decrement A
    jp nz, loop

    hlt
";

Assembler assembler = new(text);
assembler.Assemble();

foreach (string line in assembler.Lines)
{
    Console.WriteLine(line);
}