using System;
using ProtoBuf;

namespace Chat.Main.Packet.DataStructs
{
    [Serializable]
    [ProtoContract]
    public struct Text
    {
        [ProtoMember(1)] 
        public byte[] Content;

        public Text(byte[] data)
    {
        // Структура пакета текстового сообщения 
        // | Тип пакета |   Данные   |
        // |   1 байт   | 0 - x байт | 
        // |    0x54    |

       // Content = new byte[data.Length - 1];
       // Array.Copy(data, 0, Content, 0, Content.Length);
            Content = data;

    }
    }
}