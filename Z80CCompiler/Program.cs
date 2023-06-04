// See https://aka.ms/new-console-template for more information


using Z80CCompiler.Assembling;
using Z80CCompiler.Parsing;
using Z80CCompiler.Parsing.ASTNodes;
using Z80CCompiler.Parsing.Tokens;

const string code = @"int main() {
    return 2 * 5;
}
";

Lexer lexer = new(code);
List<IToken> tokens = lexer.Lex();
Console.WriteLine("[{0}]", string.Join(", ", tokens));

Parser parser = new(tokens);
AbstractSyntaxTree ast = parser.Parse();
CodeGenerator codeGenerator = new(ast);
GeneratedCode generatedCode = codeGenerator.Assemble();
_ = 0;
