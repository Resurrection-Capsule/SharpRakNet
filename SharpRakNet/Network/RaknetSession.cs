using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;

using SharpRakNet.Protocol;
using SharpRakNet.Protocol.Raknet;

namespace SharpRakNet.Network
{
    public class RaknetSession
    {
        public IPEndPoint PeerEndPoint { get; private set; }
        public ulong Guid { get; set; }

        private static readonly Dictionary<int, List<(Type, Delegate)>> Listeners = new Dictionary<int, List<(Type, Delegate)>>();

        public Thread SenderThread;
        public byte rak_version;
        public Timer PingTimer;
        public bool Connected;
        public RecvQ Recvq;
        public SendQ Sendq;

        private readonly AsyncUdpClient Socket;
        private bool thrownUnkownPackets;
        public int MaxRepingCount = 6;
        private int repingCount;

        public delegate void SessionDisconnectedDelegate(RaknetSession session);
        public SessionDisconnectedDelegate SessionDisconnected = delegate { };

        public delegate bool PacketReceiveBytesDelegate(RaknetSession session, byte[] bytes);
        public PacketReceiveBytesDelegate SessionReceiveRaw = delegate { return false; };
        public SessionDisconnectedDelegate SessionOnNewIncomingConnection = delegate { };

        public RaknetSession(AsyncUdpClient Socket, IPEndPoint Address, ulong guid, byte rakVersion, RecvQ recvQ, SendQ sendQ, bool thrownUnkownPackets = false)
        {
            this.Socket = Socket;
            PeerEndPoint = Address;
            Guid = guid;
            Connected = true;
            this.thrownUnkownPackets = thrownUnkownPackets;

            rak_version = rakVersion;

            Sendq = sendQ;
            Recvq = recvQ;

            StartPing();
            SenderThread = StartSender();
        }

        public static void Subscribe<T>(Action<RaknetSession, T> action) where T : Packet {
            Type packetType = typeof(T);

            RegisterPacketID attribute = 
                packetType.GetCustomAttributes(false).OfType<RegisterPacketID>().FirstOrDefault() ?? throw new Exception(packetType.FullName + " must have the [RegisterPacketID(int)] attribute.");

            bool hasBufferConstructor = false;
            foreach(ConstructorInfo constructor in packetType.GetConstructors())
            {
                ParameterInfo[] parameters = constructor.GetParameters();

                if (parameters.Length != 1) continue;
                if (parameters[0].ParameterType != typeof(byte[])) continue;

                hasBufferConstructor = true;
            }

            if (!hasBufferConstructor) throw new Exception(packetType.FullName + " must have a constructor that takes only a byte[].");

            int packetId = attribute.ID;

            bool exists = Listeners.TryGetValue(packetId, out var listeners);
            if(!exists)
            {
                listeners = new List<(Type, Delegate)>();
                Listeners.Add(packetId, listeners);
            }

            listeners.Add((packetType, action));
        }

        public void HandleFrameSet(IPEndPoint peer_addr, byte[] data)
        {
            if (!Connected)
            {
                foreach (FrameSetPacket f in Recvq.Flush(peer_addr))
                {
                    HandleFrame(peer_addr, f);
                }
            }

            switch ((PacketID)data[0])
            {
                case PacketID.Nack:
                    {
                        lock (Sendq)
                        {
                            Nack nack = Packet.ReadPacketNack(data);
                            for (int i = 0; i < nack.record_count; i++)
                            {
                                if (nack.sequences[i].Start == nack.sequences[i].End)
                                {
                                    Sendq.Nack(nack.sequences[i].Start, Common.CurTimestampMillis());
                                }
                                else
                                {
                                    for (uint j = nack.sequences[i].Start; j <= nack.sequences[i].End; j++)
                                    {
                                        Sendq.Nack(j, Common.CurTimestampMillis());
                                    }
                                }
                            }
                        }

                        break;
                    }
                case PacketID.Ack:
                    {
                        lock (Sendq)
                        {
                            Ack ack = Packet.ReadPacketAck(data);
                            for (int i = 0; i < ack.record_count; i++)
                            {
                                if (ack.sequences[i].Start == ack.sequences[i].End)
                                {
                                    Sendq.Ack(ack.sequences[i].Start, Common.CurTimestampMillis());
                                }
                                else
                                {
                                    for (uint j = ack.sequences[i].Start; j <= ack.sequences[i].End; j++)
                                    {
                                        Sendq.Ack(j, Common.CurTimestampMillis());
                                    }
                                }
                            }
                        }

                        break;
                    }
                default:
                    {
                        if ((PacketID)data[0] >= PacketID.FrameSetPacketBegin 
                            && (PacketID)data[0] <= PacketID.FrameSetPacketEnd)
                        {
                            var frames = new FrameVec(data);
                            lock (Recvq)
                            {
                                foreach (var frame in frames.frames)
                                {
                                    Recvq.Insert(frame);
                                    foreach (FrameSetPacket f in Recvq.Flush(peer_addr))
                                    {
                                        HandleFrame(peer_addr, f);
                                    }
                                }

                            }
                            var acks = Recvq.GetAck();
                            if (acks.Count != 0)
                            {
                                Ack packet = new Ack
                                {
                                    record_count = (ushort)acks.Count,
                                    sequences = acks,
                                };
                                byte[] buf = Packet.WritePacketAck(packet);
                                Socket.Send(peer_addr, buf);
                            }
                        }
                        break;
                    }
            }
        }

        public void HandleFrame(IPEndPoint address, FrameSetPacket frame)
        {
            PacketID packetID = (PacketID)frame.data[0];

            switch (packetID)
            {
                case PacketID.ConnectedPing:
                    HandleConnectPing(frame.data);
                    break;
                case PacketID.ConnectedPong:
                    repingCount = 0;
                    break;
                case PacketID.ConnectionRequest:
                    break;
                case PacketID.ConnectionRequestAccepted:
                    break;
                case PacketID.NewIncomingConnection:
                    SessionOnNewIncomingConnection(this);
                    break;
                case PacketID.Disconnect:
                    HandleDisconnectionNotification();
                    break;
                default:
                    if (SessionReceiveRaw(this, frame.data))
                        break;

                    if (!HandleIncomingPacket(frame.data))
                    {
                        Console.WriteLine($"RaknetSession: unhandled packet: 0x{packetID:X}");
                    }
                    break;
            }
        }

        private bool HandleIncomingPacket(byte[] buffer) {
            byte packetID = buffer[0];

            bool exists = Listeners.TryGetValue(packetID, out List<(Type, Delegate)> value);
            if (!exists)
                return false;

            var handled = false;

            foreach((Type, Delegate) registration in value)
            {
                Delegate callback = registration.Item2;
                Type packetType = registration.Item1;

                MethodInfo method = packetType.GetMethod("Deserialize");
                if (method == null)
                    continue;

                object packet = Activator.CreateInstance(packetType, new object[] { buffer });
                method.Invoke(packet, new object[] {});

                callback.DynamicInvoke(this, packet);

                handled = true;
            }

            return handled;
        }

        private void HandleConnectPing(byte[] data)
        {
            ConnectedPing pingPacket = Packet.ReadPacketConnectedPing(data);
            ConnectedPong pongPacket = new ConnectedPong
            {
                client_timestamp = pingPacket.client_timestamp,
                server_timestamp = (uint)(Common.CurTimestampMillis() & 0xFFFFFFFF),
            };

            byte[] buf = Packet.WritePacketConnectedPong(pongPacket);
            lock (Sendq)
                Sendq.Insert(Reliability.Unreliable, buf);
        }

        private void HandleDisconnectionNotification()
        {
            Disconnect();
        }

        public void Disconnect()
        {
            Connected = false;
            if (SenderThread != null && SenderThread.IsAlive) SenderThread.Join();
            PingTimer?.Dispose();
            SessionDisconnected(this);
            GC.Collect();
        }

        public void HandleConnect()
        {
            ConnectionRequest requestPacket = new ConnectionRequest
            {
                guid = Guid,
                time = (uint)(Common.CurTimestampMillis() & 0xFFFFFFFF),
                use_encryption = 0x00,
            };
            byte[] buf = Packet.WritePacketConnectionRequest(requestPacket);
            lock (Sendq)
                Sendq.Insert(Reliability.ReliableOrdered, buf);
        }

        public Thread StartSender()
        {
            Thread thread = new Thread(() => 
            {
                while (Connected)
                {
                    Thread.Sleep(5); 
                    lock (Sendq)
                    {
                        foreach (FrameSetPacket item in Sendq.Flush(Common.CurTimestampMillis(), PeerEndPoint))
                        {
                            byte[] sdata = item.Serialize();
                            Socket.Send(PeerEndPoint, sdata);
                        }
                    }
                }
            });
            thread.Start();
            return thread;
        }

        public void StartPing()
        {
            PingTimer = new Timer(SendPing, null,
                new Random().Next(1000, 1500), new Random().Next(1000, 1500));
        }

        public void SendPing(object obj)
        {
            if (!Connected) return;
            ConnectedPing pingPacket = new ConnectedPing
            {
                client_timestamp = (uint)(Common.CurTimestampMillis() & 0xFFFFFFFF),
            };

            byte[] buffer = Packet.WritePacketConnectedPing(pingPacket);
            lock (Sendq)
                Sendq.Insert(Reliability.Unreliable, buffer);

            repingCount++;
            if (repingCount < MaxRepingCount) return;

            HandleDisconnectionNotification();
        }

        public void Send(byte[] data, Reliability reliability, int channel)
        {
            lock (Sendq)
            {
                Sendq.Insert(reliability, data);
            }
        }
    }
}