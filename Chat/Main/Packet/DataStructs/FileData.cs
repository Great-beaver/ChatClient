using System;
using ProtoBuf;

namespace Chat.Main.Packet.DataStructs
{
    [Serializable]
    [ProtoContract]
    public struct FileData
    {
        [ProtoMember(1)] 
        public byte[] Content;

        [ProtoMember(2)]
        public bool LastPacket;

        [ProtoMember(3)] 
        public byte PacketNumber;

        public FileData(byte[] content, bool lastPacket, byte packetNumber)
    {
        // Структура пакета файла
        // | Тип пакета | Последний пакет | Номер пакета |  Данные  |
        // |   1 байт   |      1 байт     |    1 байт    |  0 - ... |
        // |    0x46    |

        // Тип пакета
        //Type = DataType.FileData;
     //
     //   // Устанавливает последний ли пакет
     //   LastPacket = data[0];
     //
     //   // Получает номер пакета
     //   PacketNumber = data[1];
     //
     //   // Непосредствено контент
     //   Content = new byte[data.Length - 2];
     //   // Вставляет контент в массив
     //   Array.Copy(data, 2, Content, 0, Content.Length);.

            Content = content;

            LastPacket = lastPacket;

            PacketNumber = packetNumber;



    }
}
}