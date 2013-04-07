using System;

namespace ChatClient.Main.Packet
{
    public struct Header
    {
        // Структура Header'a
        // | Сигнатура | Получатель | Отправитель | Длинна данных |  Опции   | Контрольная сумма |   Данные   | 
        // |  2 байта  |   1 байт   |   1 байт    |    2 байта    | 2 байта  |      2 байта      | 0 - x байт | 
        // | 0xAA 0x55 |

        public ushort Signature;
        public byte Recipient;
        public byte Sender;
        public ushort DataLenght;
        public byte Option1Byte;
        public byte Option2Byte;
        public PacketOption1 Option1;
        public PacketOption2 Option2;
        public ushort Crc;


        public Header(byte[] header)
        {
                    // Считывание данных для создания пакета
                    // Здесь важен строгий порядок считывания байтов, точно как в пакете.
                     Signature = 0xAA55;
                     Recipient = header[0];
                     Sender = header[1];
                     DataLenght = BitConverter.ToUInt16(new byte[] {header[2], header[3]}, 0);
                     Option1Byte = header[4];
                     Option2Byte = header[5];
                     Crc = BitConverter.ToUInt16(new byte[] { header[6], header[7] }, 0);

                     Option1 = PacketOption1.None;
                     Option2 = PacketOption2.None;

                     switch (Option1Byte)
                     {
                         case 0x06:
                             {
                                 Option1 = PacketOption1.Acknowledge;
                             }
                             break;

                         case 0x41:
                             {
                                 Option1 = PacketOption1.FileTransferAllowed;
                             }
                             break;

                         case 0x18:
                             {
                                 Option1 = PacketOption1.FileTransferDenied;
                             }
                             break;

                         case 0x04:
                             {
                                 Option1 = PacketOption1.FileTransferCompleted;
                             }
                             break;

                     }

                     switch (Option2Byte)
                     {
                         case 0x43:
                             {
                                 Option2 = PacketOption2.Compressed;
                             }
                             break;
                     }
        }

        public Header(byte recipient, byte sender, byte option1, byte option2)
        {
            Signature = 0xAA55;

            Recipient = recipient;
            Sender = sender;
            DataLenght = 0; // Заглушка
            Option1Byte = option1;
            Option2Byte = option2;

            Crc = 0; // Заглушка

            Option1 = PacketOption1.None;
            Option2 = PacketOption2.None;

            switch (option1)
            {
                case 0x06:
                    {
                        Option1 = PacketOption1.Acknowledge;
                    }
                    break;

                case 0x41:
                    {
                        Option1 = PacketOption1.FileTransferAllowed;
                    }
                    break;

                case 0x18:
                    {
                        Option1 = PacketOption1.FileTransferDenied;
                    }
                    break;

                case 0x04:
                    {
                        Option1 = PacketOption1.FileTransferCompleted;
                    }
                    break;

            }

            switch (option2)
            {
                case 0x43:
                    {
                        Option2 = PacketOption2.Compressed;
                    }
                    break;

            }


        }

    }
}