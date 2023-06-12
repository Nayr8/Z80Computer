

using Z80Assembler;
using Z80Assembler.Ast;
using Z80Assembler.Tokens;

Console.WriteLine("test");

string code =
@"section .text
_start:
ld a, 5";

List<Token> tokens = Lexer.Run(code);
Parser parserResults = new Parser(tokens);
var a = 0;