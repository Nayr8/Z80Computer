

namespace Z80CPUEmulator
{
    public class IndexRegisters : ICloneable
    {
        public ushort StackPointer { set; get; }
        public ushort IndexX { get; set; }
        public ushort IndexY { get; set; }

        public static bool operator ==(IndexRegisters lhs, IndexRegisters rhs) => lhs.Equals(rhs);

        public static bool operator !=(IndexRegisters lhs, IndexRegisters rhs) => !(lhs == rhs);

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
            IndexRegisters other = (IndexRegisters) obj;

            return StackPointer == other.StackPointer && IndexX == other.IndexX && IndexY == other.IndexY;
        }

        public object Clone()
        {
            return new IndexRegisters() {
                StackPointer = StackPointer,
                IndexX = IndexX,
                IndexY = IndexY
            };
        }
    }
}
