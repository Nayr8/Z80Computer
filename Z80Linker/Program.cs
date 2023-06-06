
using Z80Linker;string path = "../../../kernel_link_script";
char[] linkScript = File.ReadAllText(path).ToCharArray();

LinkerScript ls = new(linkScript);

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
