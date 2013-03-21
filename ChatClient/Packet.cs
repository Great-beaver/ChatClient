using System;

namespace ChatClient
{
    public struct Packet
    {
        // Структура Header'a
        // | Сигнатура | Получатель | Отправитель | Длинна данных |  Опции   | Контрольная сумма |   Данные   | 
        // |  2 байта  |   1 байт   |   1 байт    |    2 байта    | 2 байта  |      2 байта      | 0 - x байт | 
        // | 0xAA 0x55 |

        public byte[] Signature;
        public byte Recipient;
        public byte Sender;
        public ushort DataLenght;
        public byte Option1;
        public byte Option2;
        public ushort Crc;
        public byte[] Data;

        // Массив в котором собирается завершенный пакет
        private byte[] _packet;

        //Массив содержаший данные для вычисления CRC
        private byte[] _packetForCrc;

        public Packet(byte recipient, byte sender,byte option1, byte option2,byte[] data)
        {
            Signature = new byte[2];
            Signature[0] = 0xAA;
            Signature[1] = 0x55;

            Recipient = recipient;
            Sender = sender;
            DataLenght = (ushort) data.Length;
            Option1 = option1;
            Option2 = option2;

            // TO DO: лучше принимать структуру PacketData
            Data = new byte[data.Length];

            // Копирует данные
            Array.Copy(data, 0, Data, 0, data.Length);

            // TO DO: Вычислять CRC учитывая данные пакета и header, убрать отдельное вычисление CRC данных

            // Контрольная сумма высчитывется по  | Получатель | Отправитель | Длинна данных |  Опции   | + Данные
            // Без учета сигнатуры                |   1 байт   |   1 байт    |    2 байта    | 2 байта  |

            _packetForCrc = new byte[6 + DataLenght];
            _packetForCrc[0] = Recipient;
            _packetForCrc[1] = Sender;
            Array.Copy(BitConverter.GetBytes(DataLenght),0,_packetForCrc,2,2);
            _packetForCrc[4] = Option1;
            _packetForCrc[5] = Option2;
            Array.Copy(Data,0,_packetForCrc,6,DataLenght);

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
            _packet[6] = Option1;
            _packet[7] = Option2;

            //Crc
            Array.Copy(BitConverter.GetBytes(Crc),0,_packet,8,2);


            Array.Copy(Data,0,_packet,10,Data.Length);
            return _packet;
        }

    }
}