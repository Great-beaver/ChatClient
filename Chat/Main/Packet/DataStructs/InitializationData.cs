using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Chat.Main.Packet.DataStructs
{
    [Serializable]
    [ProtoContract]
    public struct InitializationData
    {
        [ProtoMember(1)]
        public readonly DateTime SycDateTime;
        [ProtoMember(2)]
        public readonly int[] EnabledClients;
        [ProtoMember(3)]
        public readonly Dictionary<int, string> Names;

        public InitializationData(DateTime timeNow, int[] clients, Dictionary<int, string> names)
        {
            SycDateTime = timeNow;
            EnabledClients = clients;
            Names = names;
        }


    }
}