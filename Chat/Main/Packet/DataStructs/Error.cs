using System;
using ProtoBuf;

namespace Chat.Main.Packet.DataStructs
{
    [Serializable]
    [ProtoContract]
    public struct Error
    {
        [ProtoMember(1)] 
        public byte[] Content;

        public Error (byte[] data)
        {
            // Добавлено чтобы избежать значения null    
            Content = data;
        }
    }
}