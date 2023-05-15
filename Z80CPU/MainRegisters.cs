using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z80CPUEmulator
{
    public class MainRegisters : ICloneable
    {
        public byte A { get; set; }
        public Flags Flags { get; set; } = new();
        public byte B { get; set; }
        public byte C { get; set; }
        public byte D { get; set; }
        public byte E { get; set; }
        public byte H { get; set; }
        public byte L { get; set; }

        public ushort AF
        {
            get => (ushort)(A << 8 | Flags.Value);
            set
            {
                A = (byte)(value >> 8);
                Flags.Value = (byte)value;
            }
        }
        public ushort BC
        {
            get => (ushort)(B << 8 | C);
            set
            {
                B = (byte)(value >> 8);
                C = (byte)value;
            }
        }
        public ushort DE
        {
            get => (ushort)(D << 8 | E);
            set
            {
                D = (byte)(value >> 8);
                E = (byte)value;
            }
        }
        public ushort HL
        {
            get => (ushort)(H << 8 | L);
            set
            {
                H = (byte)(value >> 8);
                L = (byte)value;
            }
        }

        public static bool operator ==(MainRegisters lhs, MainRegisters rhs) => lhs.Equals(rhs);

        public static bool operator !=(MainRegisters lhs, MainRegisters rhs) => !(lhs == rhs);

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }
            MainRegisters other = (MainRegisters)obj;

            return AF == other.AF && BC == other.BC && DE == other.DE && HL == other.HL;
        }

        public object Clone()
        {
            return new MainRegisters()
            {
                AF = AF,
                BC = BC,
                DE = DE,
                HL = HL,
            };
        }
    }
}
