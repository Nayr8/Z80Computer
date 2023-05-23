

using System.Text.Json.Serialization;
using Z80Assembler.Operands;

namespace Z80Assembler
{
    public class Assembler
    {
        private IEnumerable<string> _lines;
        public IEnumerable<string> Lines { get => _lines; }

        public Assembler(string code)
        {
            _lines = SanatiseInput(code);
        }

        public IEnumerable<Statement> Assemble()
        {
            List<Statement> statements = new();
            foreach (var line in _lines)
            {
                statements.Add(AssembleLine(line));
            }
            return statements;
        }

        private static Statement AssembleLine(string line)
        {
            string? label = null;
            var temp = line.Split(':');
            if (temp.Length == 2 )
            {
                label = temp[0].Trim();
                line = temp[1].Trim();
            }

            string[] statementParts = line.Split(new char[] { ' ', ',' }).Where((text) => text.Length != 0).ToArray();
            if (statementParts.Length > 3 )
            {
                throw new Exception();
            }

            IOperand[] operands = new IOperand[statementParts.Length - 1];
            if (statementParts.Length >= 2)
            {
                operands[0] = DecodeOperand(statementParts[1]);
                if (statementParts.Length >= 3)
                {
                    operands[1] = DecodeOperand(statementParts[2]);
                }
            }

            return new Statement(label, statementParts[0], operands);
        }

        private static IOperand DecodeOperand(string operand)
        {
            return operand switch
            {
                "a" => new RegisterOperand(Register.A),
                "b" => new RegisterOperand(Register.B),
                "c" => new RegisterOperand(Register.C),
                "d" => new RegisterOperand(Register.D),
                "e" => new RegisterOperand(Register.E),
                "h" => new RegisterOperand(Register.H),
                "l" => new RegisterOperand(Register.L),
                "af" => new RegisterOperand(Register.AF),
                "bc" => new RegisterOperand(Register.BC),
                "de" => new RegisterOperand(Register.DE),
                "hl" => new RegisterOperand(Register.HL),
                "sl" => new RegisterOperand(Register.SP),
                "nz" => new RegisterOperand(Register.NZ),
                "nc" => new RegisterOperand(Register.NC),
                "po" => new RegisterOperand(Register.PO),
                "p" => new RegisterOperand(Register.P),
                "z" => new RegisterOperand(Register.Z),
                "pe" => new RegisterOperand(Register.PE),
                "m" => new RegisterOperand(Register.M),
                "(af)" => new RegisterAddressOperand(Register.AF),
                "(bc)" => new RegisterAddressOperand(Register.BC),
                "(de)" => new RegisterAddressOperand(Register.DE),
                "(hl)" => new RegisterAddressOperand(Register.HL),
                "(sl)" => new RegisterAddressOperand(Register.SP),
                _ when IsValidLabel(operand) => new LabelOperand(operand),
                _ when IsAddress(operand) => new AddressOperand(ParseNumber(operand.Split('(', ')')[1])),
                _ =>  new LiteralOperand(ParseNumber(operand)),
            };
        }

        private static int ParseNumber(string numberString)
        {
            if (int.TryParse(numberString, out int number))
            {
                return number;
            }
            if (numberString.Last() == 'h')
            {
                int.Parse(numberString.Split('h')[0], System.Globalization.NumberStyles.HexNumber);
            }
            if (numberString.StartsWith("0x"))
            {
                int.Parse(numberString.Split('x')[1], System.Globalization.NumberStyles.HexNumber);
            }
            throw new NotImplementedException();
        }

        private static bool IsAddress(string operand)
        {
            return operand.First() == '(' && operand.Last() == ')';
        }

        private static bool IsValidLabel(string operand)
        {
            foreach (var letter in operand)
            {
                if (!IsAlphanumeric(letter)) return false;
            }
            return IsLetter(operand[0]);
        }

        private static bool IsAlphanumeric(char c)
        {
            return IsLetter(c) || IsDigit(c);
        }

        private static bool IsLetter(char c)
        {
            return (c > 'a' && c < 'z') || (c > 'A' && c < 'Z');
        }

        private static bool IsDigit(char c)
        {
            return c > '0' && c < '9';
        }

        private static IEnumerable<string> SanatiseInput(string input)
        {
            string[] lines = input.ToLower().Split('\n');
            List<string> newLines = new();
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim().Split(';')[0];
                if (lines[i].Length != 0)
                {
                    if (lines[i].Last() == ':')
                    {
                        newLines.Add(lines[i] + lines[i + 1].Trim().Split(';')[0]);
                        i++;
                        continue;
                    }
                    newLines.Add(lines[i]);
                }
            }
            return newLines;
        }
    }
}
