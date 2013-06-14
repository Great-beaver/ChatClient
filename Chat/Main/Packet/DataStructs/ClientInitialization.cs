using System;
using ProtoBuf;

namespace Chat.Main.Packet.DataStructs
{
    [Serializable]
    [ProtoContract]
    public struct ClientInitialization
    {
        [ProtoMember(1)]
        public readonly DateTime SycTime;
        [ProtoMember(2)]
        public readonly int[] EnabledClients;
        
        public ClientInitialization(DateTime timeNow, int[] clients)
        {
            SycTime = timeNow;
            EnabledClients = clients;
        }


    }
}