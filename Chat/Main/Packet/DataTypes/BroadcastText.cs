using System;

namespace Chat.Main.Packet.DataTypes
{
    public struct BroadcastText : IData
    {
        public DataType Type { get; private set; }
        public byte[] Content { get; set; }
        public byte LastPacket { get; private set; }
        public byte PacketNumber { get; private set; }
        public long FileLenght { get; private set; }
        public string FileName { get; private set; }

        public BroadcastText(byte[] data): this()
    {
        // Структура пакета широковешательного сообщения 
        // | Тип пакета |   Данные   |
        // |   1 байт   | 0 - x байт | 
        // |    0x42    |

        Type = DataType.BroadcastText;
        Content = new byte[data.Length - 1];
        Array.Copy(data, 1, Content, 0, Content.Length);
        
        // Добавлено чтобы избежать значения null    
        FileName = "";

    }

    }
}