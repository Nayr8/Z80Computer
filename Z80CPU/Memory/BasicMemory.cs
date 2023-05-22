using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z80CPUEmulator.Memory
{
    public class BasicMemory : IMemory
    {
        private byte[] _data = new byte[ushort.MaxValue];

        public byte ReadByte(int address)
        {
            return _data[address];
        }

        public void WriteByte(int address, byte value)
        {
            _data[address] = value;
        }

        public static bool operator ==(BasicMemory lhs, BasicMemory rhs) => lhs.Equals(rhs);
        public static bool operator !=(BasicMemory lhs, BasicMemory rhs) => !(lhs == rhs);
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

            BasicMemory other = (BasicMemory)obj;

            for (int i =  0; i < _data.Length; i++)
            {
                if (_data[i] != other._data[i]) { return false; }
            }
            return true;
        }


    }
}
