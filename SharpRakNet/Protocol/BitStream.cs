using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace SharpRakNet.Protocol
{
    public class BitStream
    {
        private List<byte> _data;
        private int _readOffset;
        private int _writeOffset;

        public int ReadOffset { get => _readOffset; set => _readOffset = value; }
        public int WriteOffset { get => _writeOffset; set => _writeOffset = value; }

        public BitStream()
        {
            _data = new List<byte>();
            _readOffset = 0;
            _writeOffset = 0;
        }

        public BitStream(byte[] buffer)
        {
            _data = new List<byte>(buffer);
            _readOffset = 0;
            _writeOffset = buffer.Length * 8;
        }

        public byte[] GetData() => _data.ToArray();
        public int GetNumberOfBitsUsed() => _writeOffset;
        public int GetNumberOfUnreadBits() => _writeOffset - _readOffset;

        public void WriteBit(bool value)
        {
            int byteIndex = _writeOffset / 8;
            int bitIndex = 7 - (_writeOffset % 8);

            if (byteIndex >= _data.Count)
            {
                _data.Add(0);
            }

            if (value)
                _data[byteIndex] |= (byte)(1 << bitIndex);
            else
                _data[byteIndex] &= (byte)~(1 << bitIndex);

            _writeOffset++;
        }

        public void WriteBits(byte[] input, int numberOfBits)
        {
            if (numberOfBits <= 0) return;

            for (int i = 0; i < numberOfBits; i++)
            {
                int inputByteIndex = i / 8;
                int inputBitIndex = 7 - (i % 8);
                bool bit = (input[inputByteIndex] & (1 << inputBitIndex)) != 0;
                WriteBit(bit);
            }
        }

        public void Write(bool value) => WriteBit(value);

        public void Write(byte value) => WriteBits(new[] { value }, 8);

        public void Write(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            WriteBits(bytes, 16);
        }

        public void Write(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            WriteBits(bytes, 32);
        }

        public void Write(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            WriteBits(bytes, 64);
        }

        public void Write(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            WriteBits(bytes, 32);
        }

        public void Write(Vector3 vec)
        {
            Write(vec.X);
            Write(vec.Y);
            Write(vec.Z);
        }

        public void Write(Quaternion quat)
        {
            Write(quat.X);
            Write(quat.Y);
            Write(quat.Z);
            Write(quat.W);
        }
        
        public void Write(string value)
        {
             byte[] strBytes = Encoding.UTF8.GetBytes(value);
             Write((ushort)strBytes.Length);
             WriteBits(strBytes, strBytes.Length * 8);
        }

        public bool ReadBit()
        {
            if (_readOffset >= _writeOffset) return false;

            int byteIndex = _readOffset / 8;
            int bitIndex = 7 - (_readOffset % 8);

            bool value = (_data[byteIndex] & (1 << bitIndex)) != 0;
            _readOffset++;
            return value;
        }

        public byte[] ReadBits(int numberOfBits)
        {
            int byteLen = (numberOfBits - 1) / 8 + 1;
            byte[] res = new byte[byteLen];
            for (int i = 0; i < numberOfBits; i++)
            {
                if (ReadBit())
                {
                    int resByteIdx = i / 8;
                    int resBitIdx = 7 - (i % 8);
                    res[resByteIdx] |= (byte)(1 << resBitIdx);
                }
            }
            return res;
        }

        public byte ReadByte() => ReadBits(8)[0];

        public ushort ReadUShort()
        {
            byte[] bytes = ReadBits(16);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }

        public uint ReadUInt()
        {
            byte[] bytes = ReadBits(32);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public ulong ReadULong()
        {
            byte[] bytes = ReadBits(64);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }
        
        public float ReadFloat()
        {
            byte[] bytes = ReadBits(32);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }
        
        public bool ReadBool() => ReadBit();
    }
}