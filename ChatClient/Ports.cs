using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatClient
{
    class Ports
    {
        private bool _continue = false;
        private SerialPort _comPort;
        private Thread _readThread;
        private int _clietnId ;
        private byte[] _readBufferHeader = new byte[6];
        private Crc16 _crc16 = new Crc16();
        byte[] _reciveMessageLenght;

        public Ports ( string comPortName, int id)
        {
            _comPort = new SerialPort();
            _comPort.PortName = comPortName;
            _comPort.BaudRate = 9600;
            _comPort.Parity = Parity.None;
            _comPort.DataBits = 8;
            _comPort.StopBits = StopBits.One;
            _comPort.Handshake = Handshake.None;
            _comPort.ReadTimeout = 500;
            _comPort.WriteTimeout = 500;

            _comPort.Encoding = Encoding.GetEncoding(28591);

            _clietnId = id;

            _readThread = new Thread(Read);
            _comPort.Open();
            _continue = true;
            _readThread.Start();
        }


        public void SendPacket(string message)
        {
            // Переводим строку в массив байтов 
            byte[] messageBody = Encoding.UTF8.GetBytes(message);
            
            //Массив всего выходного пакета, Header + тело сообщения(Data)
            byte[] outPacket = new byte[10+messageBody.Length];

            // Debug message
            MessageBox.Show("Sended message lenght "+messageBody.Length.ToString());

            // Пакет без учета сигнатуры и CRC для вычисления CRC
            byte[] packetWithOutHash = new byte[6];

            //Сигнатура header'а
            outPacket[0] = 0xAA;
            outPacket[1] = 0x55;

            //Адрес получателя
            outPacket[2] = 0x00;

            //Адрес отправителя
            outPacket[3] = 0x00;

            // Копируем тело сообщения в позицию после Header'а
            Array.Copy(messageBody,0,outPacket,10,messageBody.Length);

            // Вставляем длинну сообщения тела сообщения в Header'а
            Array.Copy(BitConverter.GetBytes(((short)messageBody.Length)), 0, packetWithOutHash, 2, 2);

            // Выставляем опции в Header
            packetWithOutHash[4] = 0x48;
            packetWithOutHash[5] = 0x49;

            // Вставляем header без CRC в начало пакета 
            Array.Copy(packetWithOutHash, 0, outPacket, 2, 6);

            // Вычисляем и вставляет CRC Header'а в пакет
            Array.Copy(_crc16.ComputeChecksumBytes(packetWithOutHash),0,outPacket,8,2);

            // just hint to array copy

           // Array.Copy(a, 1, b, 0, 3);
           // a = source array
           // 1 = start index in source array
           // b = destination array
           // 0 = start index in destination array
           // 3 = elements to copy

         //  MessageBox.Show(System.Text.Encoding.UTF8.GetString(outPacket));
         //
         //  MessageBox.Show(Convert.ToString(BitConverter.ToInt16(packetWithOutHash, 2)));



            _comPort.Write(outPacket, 0, outPacket.Length);

        }

        private void Read()
        {
            while (_continue)
            {
                if (_comPort.BytesToRead > 1 && _comPort.ReadByte() == 0xAA && _comPort.ReadByte() == 0x55)
                {
                    if (_comPort.BytesToRead > 7)
                    {
                        MessageBox.Show("Ok");

                        byte[] messageHeaderWithoutHash = new byte[6];
                        _comPort.Read(messageHeaderWithoutHash, 0, 6);

                      //  MessageBox.Show("Calculated hash = " + Convert.ToString(_crc16.ComputeChecksum(messageHeaderWithoutHash))
                      //      + "Recived hash = " + Convert.ToString(BitConverter.ToUInt16(new byte[] { (byte)_comPort.ReadByte(), (byte)_comPort.ReadByte() }, 0)));

                        ushort lenght =
                            BitConverter.ToUInt16(new byte[] { messageHeaderWithoutHash[2], messageHeaderWithoutHash[3] }, 0);

                        MessageBox.Show("Lenght of recived message " + lenght.ToString());

                        if (_crc16.ComputeChecksum(messageHeaderWithoutHash) == BitConverter.ToUInt16(new byte[] { (byte)_comPort.ReadByte(), (byte)_comPort.ReadByte() }, 0))
                       {
                           MessageBox.Show("Hash matches!");
                          // short lenght =
                         //   BitConverter.ToInt16(new byte[] { messageHeaderWithoutHash[0], messageHeaderWithoutHash[1] }, 0);

                           byte[] messageBody = new byte[lenght];

                           _comPort.Read(messageBody, 0, lenght);

                           MessageBox.Show(Encoding.UTF8.GetString(messageBody));




                       }
                       else
                       {
                          MessageBox.Show("Hash NOT matches!"); 
                       }
                         
                        
                

              //
              //          short lenght =
              //              BitConverter.ToInt16(new byte[] {(byte) _comPort.ReadByte(), (byte) _comPort.ReadByte()}, 0);
              //          _comPort.ReadByte();
              //          _comPort.ReadByte();
              //          MessageBox.Show(lenght.ToString());
              //          MessageBox.Show(
              //              _crc16.ComputeChecksum(new byte[] {(byte) _comPort.ReadByte(), (byte) _comPort.ReadByte()})
              //                    .ToString());
              //
              //          //_comPort.Read(_reciveMessageLenght, 0, 2);
              //          // = BitConverter.ToInt16(_reciveMessageLenght);
              //

                    }
                    Thread.Sleep(500);
                }
            }


        }


    }
}
