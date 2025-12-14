using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics; // Para Vector3/Quaternion se usar System.Numerics
// Ou use UnityEngine.Vector3 se for Unity, ou uma struct própria.

namespace SharpRakNet.Protocol.Game
{
    /// <summary>
    /// Simula o comportamento do BitStream C++ com __BITSTREAM_NATIVE_END (Little Endian).
    /// </summary>
    public class NativeBitStream
    {
        private List<byte> _buffer = new List<byte>();

        public byte[] GetData() => _buffer.ToArray();

        // No C++, PacketID é o primeiro byte
        public void WritePacketID(byte id)
        {
            _buffer.Add(id);
        }

        public void Write(bool value)
        {
            _buffer.Add(value ? (byte)1 : (byte)0);
        }

        public void Write(byte value)
        {
            _buffer.Add(value);
        }

        // Escrita Little Endian (Native)
        public void Write(ushort value)
        {
            _buffer.AddRange(BitConverter.GetBytes(value)); // BitConverter usa Endianness da CPU (Little em PC)
        }

        public void Write(uint value)
        {
            _buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(ulong value)
        {
            _buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(float value)
        {
            _buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void Write(double value)
        {
            _buffer.AddRange(BitConverter.GetBytes(value));
        }
        
        // Strings no Darkspore (RakNet String Compressada ou Raw?)
        // Pelo C++, parece ser raw bytes em alguns casos, mas geralmente RakNet strings têm length ushort antes.
        public void WriteString(string value)
        {
            // Assumindo formato RakNet padrão para strings: [ushort length][bytes]
            // Mas cuidado: No C++ `Write(packetType)` é simples, mas estruturas complexas podem variar.
            // Para strings fixas do C++, adapte conforme necessidade.
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            Write((ushort)bytes.Length);
            _buffer.AddRange(bytes);
        }

        // Vec3 (x, y, z floats)
        public void WriteVector3(float x, float y, float z)
        {
            Write(x);
            Write(y);
            Write(z);
        }

        // Quat (x, y, z, w floats)
        public void WriteQuaternion(float x, float y, float z, float w)
        {
            Write(x);
            Write(y);
            Write(z);
            Write(w);
        }
    }
}