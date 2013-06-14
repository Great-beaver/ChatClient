using System;
using System.Text;
using ProtoBuf;

namespace Chat.Main.Packet.DataStructs
{
    [Serializable]
    [ProtoContract]
    public struct FileRequest
    {
        [ProtoMember(1)] 
        public long FileLenght;
        [ProtoMember(2)] 
        public string FileName;

        public FileRequest(long fileLenght, string fileName)
        {
            // Структура пакета запроса на передачу файла
            // | Тип пакета |  Длина файла   | Имя файла |
            // |   1 байт   |     8 байт     |  0 - 1024 |
            // |    0x52    |

            // Тип пакета
            //Type = DataType.FileRequest;

           // // Длинна файла
           // FileLenght = BitConverter.ToInt64(data, 0);
           //
           // FileName = Encoding.UTF8.GetString(data, 8,data.Length - 8);
            FileLenght = fileLenght;
            FileName = fileName;


        }

    }
}