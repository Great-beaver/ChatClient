using System;
using Chat.Main.Packet.DataStructs;
using ProtoBuf;

namespace Chat.Main.Packet.DataTypes
{
    [Serializable]
    [ProtoContract]
    public struct BroadcastText
    {
        [ProtoMember(1)]
        public byte[] Content { get; set; }

        public BroadcastText(byte[] data): this()
    {
        // Структура пакета широковешательного сообщения 
        // | Тип пакета |   Данные   |
        // |   1 байт   | 0 - x байт | 
        // |    0x42    |

        //Type = DataType.BroadcastText;
        //Content = new byte[data.Length - 1];
        //Array.Copy(data, 1, Content, 0, Content.Length);

            Content = data;


    }

    }
}