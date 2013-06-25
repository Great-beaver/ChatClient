﻿using System;
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
        
        public InitializationData(DateTime timeNow, int[] clients)
        {
            SycDateTime = timeNow;
            EnabledClients = clients;
        }


    }
}