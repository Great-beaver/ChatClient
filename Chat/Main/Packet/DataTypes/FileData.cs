using System;

namespace ChatClient.Main.Packet.DataTypes
{
    public struct FileData : IData

{
    public DataType Type { get; private set; }
    public byte[] Content { get;  set; }
    public byte LastPacket { get; private set; }
    public byte PacketNumber { get; private set; }
    public long FileLenght { get; private set; }
    public string FileName { get; private set; }

    public FileData(byte[] data) : this()
    {
        // Структура пакета файла
        // | Тип пакета | Последний пакет | Номер пакета |  Данные  |
        // |   1 байт   |      1 байт     |    1 байт    |  0 - ... |
        // |    0x46    |

        // Тип пакета
        Type = DataType.FileData;

        // Устанавливает последний ли пакет
        LastPacket = data[1];

        // Получает номер пакета
        PacketNumber = data[2];

        // Непосредствено контент
        Content = new byte[data.Length - 3];
        // Вставляет контент в массив
        Array.Copy(data, 3, Content, 0, Content.Length);

        // Добавлено чтобы избежать значения null    
        FileName = "";

    }
}
}