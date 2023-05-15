

namespace Z80CPUEmulator
{
    public class Registers : ICloneable
    {
        public MainRegisters MainRegisters { get; set; } = new();
        public MainRegisters ShadowRegisters { get; set; } = new();
        public IndexRegisters IndexRegisters { get; set; } = new();
        public ushort ProgramCounter { get; set; }
        public byte InterruptVector { get; set; }
        public byte RefreshCounter { get; set; }

        public ushort AF
        {
            get => MainRegisters.AF;
            set => MainRegisters.AF = value;
        }
        public ushort BC
        {
            get => MainRegisters.BC;
            set => MainRegisters.BC = value;
        }
        public ushort DE
        {
            get => MainRegisters.DE;
            set => MainRegisters.DE = value;
        }
        public ushort HL
        {
            get => MainRegisters.HL;
            set => MainRegisters.HL = value;
        }
        public byte A
        {
            get => MainRegisters.A;
            set => MainRegisters.A = value;
        }
        public byte B
        {
            get => MainRegisters.B;
            set => MainRegisters.B = value;
        }
        public byte C
        {
            get => MainRegisters.C;
            set => MainRegisters.C = value;
        }
        public byte D
        {
            get => MainRegisters.D;
            set => MainRegisters.D = value;
        }
        public byte E
        {
            get => MainRegisters.E;
            set => MainRegisters.E = value;
        }
        public byte H
        {
            get => MainRegisters.H;
            set => MainRegisters.H = value;
        }
        public byte L
        {
            get => MainRegisters.L;
            set => MainRegisters.L = value;
        }
        public Flags Flags
        {
            get => MainRegisters.Flags;
            set => MainRegisters.Flags = value;
        }
        public ushort StackPointer
        {
            get => IndexRegisters.StackPointer;
            set => IndexRegisters.StackPointer = value;
        }

        public void ExchangeAF()
        {
            ushort temp = MainRegisters.AF;
            MainRegisters.AF = ShadowRegisters.AF;
            ShadowRegisters.AF = temp;
        }
        public void ExchangeDEAndHL()
        {
            ushort temp = MainRegisters.DE;
            MainRegisters.DE = MainRegisters.HL;
            MainRegisters.HL = temp;
        }
        public void ExchangeBCDEHL()
        {
            ushort temp = MainRegisters.BC;
            MainRegisters.BC = ShadowRegisters.BC;
            ShadowRegisters.BC = temp;

            temp = MainRegisters.DE;
            MainRegisters.DE = ShadowRegisters.DE;
            ShadowRegisters.DE = temp;

            temp = MainRegisters.HL;
            MainRegisters.HL = ShadowRegisters.HL;
            ShadowRegisters.HL = temp;
        }

        public static bool operator ==(Registers lhs, Registers rhs) => lhs.Equals(rhs);

        public static bool operator !=(Registers lhs, Registers rhs) => !(lhs == rhs);

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
            Registers other = (Registers) obj;

            return MainRegisters == other.MainRegisters && ShadowRegisters == other.ShadowRegisters && IndexRegisters == other.IndexRegisters && ProgramCounter == other.ProgramCounter && InterruptVector == other.InterruptVector && RefreshCounter == other.RefreshCounter;
        }

        public object Clone()
        {
            return new Registers()
            {
                MainRegisters = (MainRegisters)MainRegisters.Clone(),
                ShadowRegisters = (MainRegisters)ShadowRegisters.Clone(),
                IndexRegisters = (IndexRegisters)IndexRegisters.Clone(),
                ProgramCounter = ProgramCounter,
                InterruptVector = InterruptVector,
                RefreshCounter = RefreshCounter
            };
        }
    }
}
