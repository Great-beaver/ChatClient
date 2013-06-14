using System;
using Chat.Main.Packet.DataTypes;
using ProtoBuf;

namespace Chat.Main.Packet.DataStructs
{
    [Serializable]
    [ProtoContract]
    public struct Data
    {
        [ProtoMember(1)]
        public DataType Type;
        [ProtoMember(2)]
        public byte[] Content;

        public Data (DataType type, byte[] content)
        {
            Type = type;
            Content = content;
        }

    }
}