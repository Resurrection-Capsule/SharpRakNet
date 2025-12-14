using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using SharpRakNet.Protocol;
using SharpRakNet.Protocol.Raknet;

namespace SharpRakNet
{
    public class RaknetWriter
    {
        private List<byte> buf = new List<byte>();

        public void Write(byte[] v)
        {
            buf.AddRange(v);
        }


        public void WriteU8(PacketID id) {
            WriteU8((byte)id);
        }

        public void WriteU8(byte v)
        {
            buf.Add(v);
        }

        public void WriteI16(short v, Endian n)
        {
            byte[] bytes;
            if (n == Endian.Big)
            {
                bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(v));
            }
            else
            {
                bytes = BitConverter.GetBytes(v);
            }
            buf.AddRange(bytes);
        }

        public void WriteU16(ushort v, Endian n)
        {
            byte[] bytes;
            if (n == Endian.Big)
            {
                bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)v));
            }
            else
            {
                bytes = BitConverter.GetBytes(v);
            }
            buf.AddRange(bytes);
        }

        public void WriteU24(uint v, Endian n)
        {
            byte[] bytes;
            if (n == Endian.Big)
            {
                bytes = new byte[]
                {
                (byte)((v >> 16) & 0xFF),
                (byte)((v >> 8) & 0xFF),
                (byte)(v & 0xFF)
                };
            }
            else
            {
                bytes = new byte[]
                {
                (byte)(v & 0xFF),
                (byte)((v >> 8) & 0xFF),
                (byte)((v >> 16) & 0xFF)
                };
            }
            buf.AddRange(bytes);
        }

        public void WriteI32(int v, Endian n)
        {
            byte[] bytes;
            if (n == Endian.Big)
            {
                bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(v));
            }
            else
            {
                bytes = BitConverter.GetBytes(v);
            }
            buf.AddRange(bytes);
        }

        public void WriteU32(uint v, Endian n)
        {
            byte[] bytes;
            if (n == Endian.Big)
            {
                bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)v));
            }
            else
            {
                bytes = BitConverter.GetBytes(v);
            }
            buf.AddRange(bytes);
        }

        public void WriteI64(long v, Endian n)
        {
            byte[] bytes;
            if (n == Endian.Big)
            {
                bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(v));
            }
            else
            {
                bytes = BitConverter.GetBytes(v);
            }
            buf.AddRange(bytes);
        }

        public void WriteMagic()
        {
            byte[] magic = new byte[]
            {
            0x00, 0xff, 0xff, 0x00, 0xfe, 0xfe, 0xfe, 0xfe,
            0xfd, 0xfd, 0xfd, 0xfd, 0x12, 0x34, 0x56, 0x78
            };
            buf.AddRange(magic);
        }

        public void WriteU64(ulong v, Endian n)
        {
            byte[] bytes;
            if (n == Endian.Big)
            {
                bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((long)v));
            }
            else
            {
                bytes = BitConverter.GetBytes(v);
            }
            buf.AddRange(bytes);
        }

        public void WriteString(string body)
        {
            byte[] raw = System.Text.Encoding.UTF8.GetBytes(body);
            WriteU16((ushort)raw.Length, Endian.Big);
            buf.AddRange(raw);
        }

        public void WriteAddress(IPEndPoint address)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                WriteU8(0x04); // 1 byte: IPv4 indicator
                
                byte[] ipBytes = address.Address.GetAddressBytes(); // 4 bytes
                for (int i = 0; i < ipBytes.Length; i++)
                {
                    WriteU8((byte)(0xFF - ipBytes[i])); // Complemento de 1
                }
                
                WriteU16((ushort)address.Port, Endian.Big); // 2 bytes: porta
                
                // Total: 1 + 4 + 2 = 7 bytes
            }
            else
            {
                // IPv6 (não usado aqui)
                throw new NotImplementedException("IPv6 not supported");
            }
        }

        public void WriteSequences(List<AckRange> sequences)
        {
            WriteU16((ushort)sequences.Count, Endian.Big);

            foreach (var sequence in sequences)
            {
                // Check if Start and End properties are not null before accessing them
                if (sequence != null)
                {
                    byte singleSequenceNumber = (byte)(sequence.Start == sequence.End ? 0x01 : 0x00);
                    WriteU8(singleSequenceNumber);
                    WriteU24(sequence.Start, Endian.Little);

                    if (singleSequenceNumber == 0x00)
                    {
                        WriteU24(sequence.End, Endian.Little);
                    }
                }
            }
        }

        public byte[] GetRawPayload()
        {
            return buf.ToArray();
        }

        public ulong Position
        {
            get { return (ulong)buf.Count; }
        }
    }

    public enum Endian
    {
        Big,
        Little
    }
}
