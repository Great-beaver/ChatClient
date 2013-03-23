using System;

namespace ChatClient.Main.Packet.DataTypes
{
    public struct TextData : IData
    {
        public string Type { get; private set; }
        public byte[] Content { get; set; }
        public byte LastPacket { get; private set; }
        public byte PacketNumber { get; private set; }
        public long FileLenght { get; private set; }
        public string FileName { get; private set; }

        public TextData(byte[] data) : this()
    {
        // Структура пакета текстового сообщения 
        // | Тип пакета |   Данные   |
        // |   1 байт   | 0 - x байт | 
        // |    0x54    |

        Type = "Text";
        Content = new byte[data.Length - 1];
        Array.Copy(data, 1, Content, 0, Content.Length);
        
        // Добавлено чтобы избежать значения null    
        FileName = "";

    }
    }
}