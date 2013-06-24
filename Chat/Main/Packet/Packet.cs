using System;
using Chat.Helpers;
using Chat.Main.Packet.DataStructs;
using Chat.Main.Packet.DataTypes;

namespace Chat.Main.Packet
{
    public struct Packet
    {
        // Структура Packet'a
        // | Сигнатура | Получатель | Отправитель | Длинна данных |  Опции   | Контрольная сумма |   Данные   | 
        // |  2 байта  |   1 байт   |   1 байт    |    2 байта    | 2 байта  |      2 байта      | 0 - x байт | 
        // | 0xAA 0x55 |

        public Header Header;
        public readonly byte[] ByteData;
        public readonly Data Data;

        // Массив в котором собирается завершенный пакет
        private byte[] _packet;

        //Массив содержаший данные для вычисления CRC
        private byte[] _packetForCrc;

     // Конуструктор приемник
     public Packet(Header header, byte[] data)
     {
         //TO DO: Валидация входных данных
         Header = header;

         Header.DataLenght = (ushort)data.Length;

         // Разархивирование данных
         var unZipData = Compressor.Unzip(data);

         // Создание объекта данных 
         Data = StructConvertor.FromBytes<Data>(unZipData);
            
         // Запись архивированных данных для отправки подверждения
         ByteData = data;
         
         // Контрольная сумма высчитывется по  | Получатель | Отправитель | Длинна данных |  Опции   | + Данные
         // Без учета сигнатуры                |   1 байт   |   1 байт    |    2 байта    | 2 байта  |
         
         _packetForCrc = new byte[6 + Header.DataLenght];
         _packetForCrc[0] = Header.Recipient;
         _packetForCrc[1] = Header.Sender;
         Array.Copy(BitConverter.GetBytes(Header.DataLenght), 0, _packetForCrc, 2, 2);
         _packetForCrc[4] = Header.Option1Byte;
         _packetForCrc[5] = Header.Option2Byte;
         Array.Copy(ByteData, 0, _packetForCrc, 6, Header.DataLenght);
         
         Header.Crc = Crc16.ComputeChecksum(_packetForCrc);
         
         // Массив для отправки
         _packet = new byte[10 + Header.DataLenght];          
     }

     // Конструктор передатчик
        public Packet(Header header, object data)
        {
            Data = new Data(DataType.Error, StructConvertor.GetBytes(data));

            if (data is Text)
            {
                Data.Type = DataType.Text;
            }

            if (data is BroadcastText)
            {
                Data.Type = DataType.BroadcastText;
            }

            if (data is FileRequest)
            {
                Data.Type = DataType.FileRequest;
            }

            if (data is FileData)
            {
                Data.Type = DataType.FileData;
            }

            if (data is InitializationData)
            {
                Data.Type = DataType.Initialization;
            }

            ByteData = Compressor.Zip(StructConvertor.GetBytes(Data));

            Header = header;

            Header.DataLenght = (ushort)ByteData.Length;

            // Контрольная сумма высчитывется по  | Получатель | Отправитель | Длинна данных |  Опции   | + Данные
            // Без учета сигнатуры                |   1 байт   |   1 байт    |    2 байта    | 2 байта  |

            _packetForCrc = new byte[6 + Header.DataLenght];
            _packetForCrc[0] = Header.Recipient;
            _packetForCrc[1] = Header.Sender;
            Array.Copy(BitConverter.GetBytes(Header.DataLenght), 0, _packetForCrc, 2, 2);
            _packetForCrc[4] = Header.Option1Byte;
            _packetForCrc[5] = Header.Option2Byte;
            Array.Copy(ByteData, 0, _packetForCrc, 6, Header.DataLenght);

            Header.Crc = Crc16.ComputeChecksum(_packetForCrc);

            // Массив для отправки
            _packet = new byte[10 + Header.DataLenght];
        }

        public string PacketInfo ()
        {
            return "Packet info:" + '\n' +
                   "Получатель: " + Header.Recipient + '\n' +
                   "Отправитель :" + Header.Sender + '\n' +
                   "Длинна данных: " + Header.DataLenght + '\n' +
                   "Опция 1: " + Header.Option1 + '\n' +
                   "Опция 2: " + Header.Option2 + '\n' +
                   "CRC пакета: " + Header.Crc + '\n';
        }

        public byte[] ToByte()
        {
            // Всталяет сигнатуру
            _packet[0] = 0xAA;
            _packet[1] = 0x55;

            // Отправитель получатель
            _packet[2] = Header.Recipient;
            _packet[3] = Header.Sender;

            // Длинна данных в позиции 4-5
            Array.Copy(BitConverter.GetBytes(Header.DataLenght), 0, _packet, 4, 2);

            // Опции
            _packet[6] = Header.Option1Byte;
            _packet[7] = Header.Option2Byte;

            //Crc
            Array.Copy(BitConverter.GetBytes(Header.Crc), 0, _packet, 8, 2);

            Array.Copy(ByteData, 0, _packet, 10, ByteData.Length);
            return _packet;
        }
    }
}