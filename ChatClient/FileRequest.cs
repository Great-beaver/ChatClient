using System;

namespace ChatClient
{
    public struct FileRequest
    {
        public readonly string Type;
        public readonly byte[] Content;
        public readonly long FileLenght;

        public FileRequest(byte[] data)
        {
            // Структура пакета запроса на передачу файла
            // | Тип пакета |  Длина файла   | Имя файла |
            // |   1 байт   |     8 байт     |  0 - 1024 |
            // |    0x52    |

            // Тип пакета
            Type = "FileRequest";

            // Длинна файла
            FileLenght = BitConverter.ToInt64(data, 1);

            // Непосредствено контент
            Content = new byte[data.Length - 9];
            // Вставляет контент в массив
            Array.Copy(data, 9, Content, 0, Content.Length);
        }

    }
}