﻿// See https://aka.ms/new-console-template for more information


using Z80Assembler;

const string code = @"
@zero 0

start: ; The start label


; end comment";


Assembler assembler = new(code);
assembler.Tokenize();
assembler.DumpErrors();
assembler.DumpTokens();