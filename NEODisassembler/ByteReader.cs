using System;
using System.Collections.Generic;
using System.Text;

namespace NEODisassembler
{
    class ByteReader
    {
        public byte[] data;
        public int addr = 0;

        public ByteReader(byte[] data)
        {
            this.data = data;
        }

        public OpCode ReadOP()
        {
            var op = (OpCode)this.data[this.addr];
            this.addr++;
            return op;
        }

        public byte[] ReadBytes(int count)
        {
            if (this.data.Length - this.addr < count)
                count = this.data.Length - this.addr;

            var _data = new byte[count];
            for (var i = 0; i < count; i++)
                _data[i] = this.data[this.addr + i];
            this.addr += count;
            return _data;
        }

        public byte ReadByte()
        {
            var b = this.data[this.addr];
            this.addr++;
            return b;
        }
        public UInt16 ReadUInt16()
        {
            byte[] _data = new byte[2];
            _data[0] = this.ReadByte();
            _data[1] = this.ReadByte();
            return BitConverter.ToUInt16(_data, 0);
        }

        public Int16 ReadInt16()
        {
            byte[] _data = new byte[2];
            _data[0] = this.ReadByte();
            _data[1] = this.ReadByte();
            return BitConverter.ToInt16(_data, 0);
        }
        public UInt32 ReadUInt32()
        {
            byte[] _data = new byte[4];
            _data[0] = this.ReadByte();
            _data[1] = this.ReadByte();
            _data[2] = this.ReadByte();
            _data[3] = this.ReadByte();
            return BitConverter.ToUInt32(_data, 0);
        }

        public Int32 ReadInt32()
        {
            byte[] _data = new byte[4];
            _data[0] = this.ReadByte();
            _data[1] = this.ReadByte();
            _data[2] = this.ReadByte();
            _data[3] = this.ReadByte();
            return BitConverter.ToInt32(_data, 0);
        }
        public UInt64 ReadUInt64()
        {
            byte[] _data = new byte[8];
            _data[0] = this.ReadByte();
            _data[1] = this.ReadByte();
            _data[2] = this.ReadByte();
            _data[3] = this.ReadByte();
            _data[4] = this.ReadByte();
            _data[5] = this.ReadByte();
            _data[6] = this.ReadByte();
            _data[7] = this.ReadByte();
            return BitConverter.ToUInt64(_data, 0);
        }
        public Int64 ReadInt64()
        {
            byte[] _data = new byte[8];
            _data[0] = this.ReadByte();
            _data[1] = this.ReadByte();
            _data[2] = this.ReadByte();
            _data[3] = this.ReadByte();
            _data[4] = this.ReadByte();
            _data[5] = this.ReadByte();
            _data[6] = this.ReadByte();
            _data[7] = this.ReadByte();
            return BitConverter.ToInt64(_data, 0);
        }
        public byte[] ReadVarBytes()
        {
            var count = this.ReadVarInt();
            return this.ReadBytes(count);
        }

        public int ReadVarInt()
        {
            var fb = this.ReadByte();
            int value;
            if (fb == 0xFD)
                value = this.ReadUInt16();
            else if (fb == 0xFE)
                value = (int)this.ReadUInt32();
            else if (fb == 0xFF)
                value = (int)this.ReadUInt64();
            else
                value = fb;
            return value;
        }
        public bool End()
        {
            return this.addr >= this.data.Length;
        }
    }
}
