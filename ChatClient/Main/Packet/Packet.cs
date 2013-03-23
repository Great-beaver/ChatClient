using System;
using System.Windows.Forms;
using ChatClient.Main.Packet.DataTypes;

namespace ChatClient.Main.Packet
{
    public struct Packet
    {
        // Структура Header'a
        // | Сигнатура | Получатель | Отправитель | Длинна данных |  Опции   | Контрольная сумма |   Данные   | 
        // |  2 байта  |   1 байт   |   1 байт    |    2 байта    | 2 байта  |      2 байта      | 0 - x байт | 
        // | 0xAA 0x55 |

        public readonly byte[] Signature;
        public readonly byte Recipient;
        public readonly byte Sender;
        public readonly ushort DataLenght;
        public readonly byte Option1Byte;
        public readonly byte Option2Byte;
        public readonly string Option1String;
        public readonly string Option2String;
        public readonly ushort Crc;
        public readonly byte[] ByteData;
        public readonly IData Data;

        // Массив в котором собирается завершенный пакет
        private byte[] _packet;

        //Массив содержаший данные для вычисления CRC
        private byte[] _packetForCrc;

        public Packet(byte recipient, byte sender,byte option1, byte option2,byte[] data)
        {
            //TO DO: Валидация входных данных

            Signature = new byte[2];
            Signature[0] = 0xAA;
            Signature[1] = 0x55;

            Recipient = recipient;
            Sender = sender;
            DataLenght = (ushort) data.Length;
            Option1Byte = option1;
            Option2Byte = option2;

            Option1String = option1.ToString();
            Option2String = option2.ToString();

            switch (option1)
            {
                case 0x06:
                    {
                        Option1String = "ACK";
                    }
                    break;

                case 0x41:
                    {
                        Option1String = "FileTransferAllowed";
                    }
                    break;
            }

            switch (option2)
            {
                case 0x43:
                    {
                        Option2String = "Compressed";
                    }
                    break;

            }

            // Значение по умолчанию
            Data = new DataError(data);

            // | Тип пакета |   Данные   |
            // |   1 байт   | 0 - x байт | 
            if (data[0] == 0x54 && data.Length >= 1)
            {
                    Data = new TextData(data);

            }

            // | Тип пакета |  Длина файла   | Имя файла |
            // |   1 байт   |     8 байт     |  0 - 1024 |
            if (data[0] == 0x52 && data.Length >= 9)
            {
                Data = new FileRequest(data);
            }

            // | Тип пакета | Последний пакет | Номер пакета |  Данные  |
            // |   1 байт   |      1 байт     |    1 байт    |  0 - ... |
            if (data[0] == 0x46 && data.Length >= 3)
            {
                Data = new FileData(data);
            }

            ByteData = new byte[data.Length];

            // Копирует данные
            Array.Copy(data, 0, ByteData, 0, data.Length);

            // Контрольная сумма высчитывется по  | Получатель | Отправитель | Длинна данных |  Опции   | + Данные
            // Без учета сигнатуры                |   1 байт   |   1 байт    |    2 байта    | 2 байта  |

            _packetForCrc = new byte[6 + DataLenght];
            _packetForCrc[0] = Recipient;
            _packetForCrc[1] = Sender;
            Array.Copy(BitConverter.GetBytes(DataLenght),0,_packetForCrc,2,2);
            _packetForCrc[4] = Option1Byte;
            _packetForCrc[5] = Option2Byte;
            Array.Copy(ByteData, 0, _packetForCrc, 6, DataLenght);

            Crc = Crc16.ComputeChecksum(_packetForCrc);

            // Массив для отправки
            _packet = new byte[10 + data.Length];          
        }

        public byte[] ToByte()
        {
            // Всталяет сигнатуру
            _packet[0] = 0xAA;
            _packet[1] = 0x55;

            // Отправитель получатель
            _packet[2] = Recipient;
            _packet[3] = Sender;

            // Длинна данных в позиции 4-5
            Array.Copy(BitConverter.GetBytes(DataLenght), 0, _packet, 4, 2);

            // Опции
            _packet[6] = Option1Byte;
            _packet[7] = Option2Byte;

            //Crc
            Array.Copy(BitConverter.GetBytes(Crc),0,_packet,8,2);

            Array.Copy(ByteData, 0, _packet, 10, ByteData.Length);
            return _packet;
        }
    }
}