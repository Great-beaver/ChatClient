using System;

namespace ChatClient
{
    public struct FileData
    {
        public readonly string Type;
        public readonly byte[] Content;
        public readonly byte LastPacket;
        public readonly byte PacketNumber;

        public FileData(byte[] data)
        {
            // Структура пакета файла
                    // | Тип пакета | Последний пакет | Номер пакета |  Данные  |
                    // |   1 байт   |      1 байт     |    1 байт    |  0 - ... |
                    // |    0x46    |

            // Тип пакета
            Type = "FileData";
            
            // Устанавливает последний ли пакет
            LastPacket = data[1];

            // Получает номер пакета
            PacketNumber = data[2];

            // Непосредствено контент
            Content = new byte[data.Length - 3];
            // Вставляет контент в массив
            Array.Copy(data, 3, Content, 0, Content.Length);

        }
    }
}