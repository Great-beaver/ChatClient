using System;
using ProtoBuf;

namespace Chat.Main.Packet.DataStructs
{
    [Serializable]
    [ProtoContract]
    public struct Acknowledge
    {
        [ProtoMember(1)] 
        public ushort DeliveredPacketCrc;

        public Acknowledge(ushort packetCrc)
        {
            DeliveredPacketCrc = packetCrc;
        }

    }
}