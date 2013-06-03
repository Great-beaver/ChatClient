using System;
using System.Text;

namespace Chat.Main.Packet.DataTypes
{
    public struct FileRequest : IData
    {
        public DataType Type { get; private set; }
        public byte[] Content { get;  set; }
        public byte LastPacket { get; private set; }
        public byte PacketNumber { get; private set; }
        public long FileLenght { get; private set; }
        public string FileName { get; private set; }

        public FileRequest(byte[] data) : this()
        {
            // Структура пакета запроса на передачу файла
            // | Тип пакета |  Длина файла   | Имя файла |
            // |   1 байт   |     8 байт     |  0 - 1024 |
            // |    0x52    |

            // Тип пакета
            Type = DataType.FileRequest;

            // Длинна файла
            FileLenght = BitConverter.ToInt64(data, 1);

            FileName = Encoding.UTF8.GetString(data, 9,data.Length - 9);

            // Добавлено чтобы избежать значения null  
            Content = new byte[] {0x00};
        }

    }
}