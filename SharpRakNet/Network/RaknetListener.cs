using SharpRakNet.Protocol;
using SharpRakNet.Protocol.Raknet;

using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Net;
using System;

namespace SharpRakNet.Network
{
    public class RaknetListener
    {
        public delegate void SessionConnectedDelegate(RaknetSession session);
        public SessionConnectedDelegate SessionConnected = delegate { };
        public SessionConnectedDelegate SessionDisconnected = delegate { };
        public AsyncUdpClient Socket;

        private static readonly Dictionary<int, List<(Type, Delegate)>> Listeners = new Dictionary<int, List<(Type, Delegate)>>();
        private Dictionary<IPEndPoint, RaknetSession> Sessions = new Dictionary<IPEndPoint, RaknetSession>();

        private byte rak_version = 0xA;
        private ulong guid;

        private static void Log(string message)
            => Console.WriteLine($"[RakNet]: {message}");

        public RaknetListener(IPEndPoint address)
        {
            Socket = new AsyncUdpClient(address);
            Socket.PacketReceived += OnPacketReceived;
            SessionConnected += OnSessionEstablished;
            guid = (ulong)(new Random().NextDouble() * ulong.MaxValue);
        }

        public void Subscribe<T>(Action<IPEndPoint, T> action) where T : Packet
        {
            Type packetType = typeof(T);

            RegisterPacketID attribute =
                packetType.GetCustomAttributes(false).OfType<RegisterPacketID>().FirstOrDefault() ?? throw new Exception(packetType.FullName + " must have the RegisterPacketID attribute.");

            bool hasBufferConstructor = false;
            foreach (ConstructorInfo constructor in packetType.GetConstructors())
            {
                ParameterInfo[] parameters = constructor.GetParameters();

                if (parameters.Length != 1) continue;
                if (parameters[0].ParameterType != typeof(byte[])) continue;

                hasBufferConstructor = true;
            }

            if (!hasBufferConstructor) throw new Exception(packetType.FullName + " must have a constructor that takes only a byte[].");

            int packetId = attribute.ID;

            bool exists = Listeners.TryGetValue(packetId, out var listeners);
            if (!exists)
            {
                listeners = new List<(Type, Delegate)>();
                Listeners.Add(packetId, listeners);
            }

            listeners.Add((packetType, action));
        }

        private void OnSessionEstablished(RaknetSession session)
        {
            session.SessionDisconnected += RemoveSession;
        }

        void RemoveSession(RaknetSession session)
        {
            IPEndPoint peerAddr = session.PeerEndPoint;

            SessionDisconnected(session);

            lock (Sessions)
                Sessions.Remove(peerAddr);
        }

        private void OnPacketReceived(IPEndPoint address, byte[] data)
        {
            var packetId = (PacketID)data[0];

            Console.WriteLine($"[RakNet Debug]: Packet ID: {packetId} (0x{(byte)packetId:X2}) from {address}");

            switch (packetId)
            {
                case PacketID.OpenConnectionRequest1:
                    HandleOpenConnectionRequest1(address, data);
                    break;

                case PacketID.OpenConnectionRequest2:
                    HandleOpenConnectionRequest2(address, data);
                    break;

                case PacketID.ConnectionRequest:
                    Log($"ConnectionRequest (0x09) received.");
                    HandleConnectionRequest(address, data);
                    break;

                case PacketID.NewIncomingConnection:
                    Log($"NewIncomingConnection (0x13) received.");
                    HandleNewIncomingConnection(address, data);
                    break;

                default:
                {
                    if (Sessions.TryGetValue(address, out var session))
                    {
                        HandleIncomingPacket(address, data);
                        session.HandleFrameSet(address, data);
                    }
                    else
                    {
                        Log($"Packet 0x{(byte)packetId:X2} dropped: no session for {address}");
                    }
                    break;
                }
            }
        }

        private void HandleConnectionRequest(IPEndPoint address, byte[] data)
        {
            try
            {
                var connectionRequest = Packet.ReadPacketConnectionRequest(data);

                RaknetSession session = null;
                bool isNewSession = false;

                lock (Sessions)
                {
                    if (!Sessions.TryGetValue(address, out session))
                    {
                        RecvQ recvQ = new RecvQ();
                        SendQ sendQ = new SendQ(Common.RAKNET_CLIENT_MTU);
                        
                        session = new RaknetSession(
                            Socket, 
                            address, 
                            guid, 
                            rak_version, 
                            recvQ, 
                            sendQ, 
                            true
                        );

                        Sessions.Add(address, session);
                        isNewSession = true;
                        
                        Log($"New session created: {address} (guid: {connectionRequest.guid:X16})");
                    }
                    else
                    {
                        Log($"Retransmitting handshake (0x10) to existing session: {address}");
                    }
                }

                var acceptedPacket = new ConnectionRequestAccepted
                {
                    client_address = address,
                    system_index = 0,
                    request_timestamp = connectionRequest.time,
                    accepted_timestamp = (uint)(Common.CurTimestampMillis() & 0xFFFFFFFF)
                };

                byte[] buffer = Packet.WritePacketConnectionRequestAccepted(acceptedPacket);
                
                lock (session.Sendq)
                {
                    session.Sendq.Insert(Reliability.Reliable, buffer);
                }

                Log($"Handshake (0x10) sent via Session (Reliable) to {address}");
            }
            catch (Exception ex)
            {
                Log($"Handshake error: {ex.Message}");
            }
        }

        private void HandleNewIncomingConnection(IPEndPoint address, byte[] data)
        {
            try
            {
                var newIncomingPacket = Packet.ReadPacketNewIncomingConnection(data);

                RaknetSession? session = null;
                lock (Sessions)
                {
                    if (!Sessions.TryGetValue(address, out session))
                    {
                        Log($"NewIncomingConnection from unknown address: {address}");
                        return;
                    }
                }

                Log($"[RakNetServer] >>> SESSION ESTABLISHED <<< From: {address}");
                SessionConnected?.Invoke(session);
            }
            catch (Exception ex)
            {
                Log($"NewIncomingConnection error: {ex.Message}");
            }
        }

        private void HandleIncomingPacket(IPEndPoint address, byte[] buffer)
        {
            byte packetID = buffer[0];

            bool exists = Listeners.TryGetValue(packetID, out List<(Type, Delegate)> value);
            if (!exists) return;

            foreach ((Type, Delegate) registration in value)
            {
                Delegate callback = registration.Item2;
                Type packetType = registration.Item1;

                MethodInfo method = packetType.GetMethod("Deserialize");
                if (method == null) return;

                object packet = Activator.CreateInstance(packetType, new object[] { buffer });
                method.Invoke(packet, new object[] {});

                callback.DynamicInvoke(address, packet);
            }
        }

        private void HandleOpenConnectionRequest1(IPEndPoint peer_addr, byte[] data)
        {
            OpenConnectionReply1 reply1Packet = new OpenConnectionReply1
            {
                magic = true,
                guid = guid,
                use_encryption = 0x00,
                mtu_size = Common.RAKNET_CLIENT_MTU,
            };
            byte[] reply1Buf = Packet.WritePacketConnectionOpenReply1(reply1Packet);
            Socket.Send(peer_addr, reply1Buf);
        }

        private void HandleOpenConnectionRequest2(IPEndPoint peer_addr, byte[] data)
        {
            OpenConnectionRequest2 req = Packet.ReadPacketConnectionOpenRequest2(data);
            OpenConnectionReply2 reply2Packet = new OpenConnectionReply2
            {
                magic = true,
                guid = guid,
                address = peer_addr,
                mtu = req.mtu,
                encryption_enabled = 0x00,
            };
            byte[] reply2Buf = Packet.WritePacketConnectionOpenReply2(reply2Packet);
            Socket.Send(peer_addr, reply2Buf);
        }

        public void BeginListener()
        {
            Socket.Run();
        }

        public void StopListener()
        {
            lock (Sessions)
            {
                for (int i = Sessions.Count - 1; i >= 0; i--)
                {
                    var session = Sessions.Values.ElementAt(i);
                    session.SessionDisconnected(session);
                }
            }

            Socket.Stop();
        }
        private class PendingConnection
        {
            public IPEndPoint Address { get; set; }
            public ulong ClientGuid { get; set; }
            public uint RequestTimestamp { get; set; }
            public byte UseEncryption { get; set; }
        }
    }
}