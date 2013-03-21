using System;

namespace ChatClient
{
    public struct TextData
    {
        public readonly string Type;
        public readonly byte[] Content;

        public TextData(byte[] data)
        {
            // Структура пакета текстового сообщения 
            // | Тип пакета |   Данные   |
            // |   1 байт   | 0 - x байт | 
            // |    0x54    |

                Type = "Text";
                Content = new byte[data.Length - 1];
                Array.Copy(data, 1, Content, 0, Content.Length);
        }
    }
}