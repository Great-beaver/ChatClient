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
        private byte _clietnId;
        private byte[] _readBufferHeader = new byte[6];
        private Crc16 _crc16 = new Crc16();
        byte[] _reciveMessageLenght;

        public bool Debug = false;

        // Хранит состояния был ли доставлен последний отправленый пакет конкретному клиенту
        private bool[] _sendedPacketDelivered = new bool[5];
        
        public Ports(string comPortName, byte id)
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

            // Пакет разрешается отправить только если значение равно true
            // При отправке пакета значение устанавливается в false
            // Пакет должен стать true после получения подтверждения о доставке или отмены доставки
            _sendedPacketDelivered[0] = true;
            _sendedPacketDelivered[1] = true;
            _sendedPacketDelivered[2] = true;
            _sendedPacketDelivered[3] = true;
            _sendedPacketDelivered[4] = true;

            // Особая уличная кодировка для правильной отправки байтов чьё значение больше 127-ми
            _comPort.Encoding = Encoding.GetEncoding(28591);

            _clietnId = id;

            _readThread = new Thread(Read);
            _comPort.Open();
            _continue = true;
            _readThread.Start();
        }


        public void SendTextMessage(string message, byte toId)
        {
            // Переводит строку в массив байтов
            byte[] messageBody = Encoding.UTF8.GetBytes(message);

            // Массив байтов для отправки
            byte[] messagePacket = new byte[messageBody.Length+3];

            // Задает тип пакета
            messagePacket[0] = 0x54;

            // Вычисляет и вставляет CRC Header'а в пакет
            Array.Copy(_crc16.ComputeChecksumBytes(messageBody), 0, messagePacket, 1, 2);

            // Копирует тело сообщения в позицию после Header'а
            Array.Copy(messageBody, 0, messagePacket, 3, messageBody.Length);

            SendPacket(messagePacket, toId);


        }


        public void SendPacket(string message, byte toId, byte option1 = 0x00, byte option2 = 0x00)
        {
        // Переводит строку в массив байтов 
            byte[] messageBody = Encoding.UTF8.GetBytes(message);

            SendPacket(messageBody, toId, option1, option2);
        }


        public bool SendPacket(byte[] messageBody, byte toId, byte option1 = 0x00, byte option2 = 0x00)
        {
            // Если ожидается доставка предыдущего пакета то сообщение не будет отправлено
            if (!_sendedPacketDelivered[toId] )
            {
                MessageBox.Show("HERE");
                return false;
            }


            // Если указанный id не предусмотрен
            if (toId > _sendedPacketDelivered.Length)
            {
                MessageBox.Show("Клиента с таким id не существует");
                return false;
            }

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
            outPacket[2] = toId;

            //Адрес отправителя
            outPacket[3] = _clietnId;

            // Копирует тело сообщения в позицию после Header'а
            Array.Copy(messageBody,0,outPacket,10,messageBody.Length);

            // Вставляет длинну сообщения тела сообщения в Header'а
            Array.Copy(BitConverter.GetBytes(((short)messageBody.Length)), 0, packetWithOutHash, 2, 2);

            // Выставляет опции в Header
            packetWithOutHash[4] = option1;
            packetWithOutHash[5] = option2;

            // Вставляет header без CRC в начало пакета 
            Array.Copy(packetWithOutHash, 0, outPacket, 2, 6);

            // Вычисляет и вставляет CRC Header'а в пакет
            Array.Copy(_crc16.ComputeChecksumBytes(packetWithOutHash),0,outPacket,8,2);

            // just hint to array copy

           // Array.Copy(a, 1, b, 0, 3);
           // a = source array
           // 1 = start index in source array
           // b = destination array
           // 0 = start index in destination array
           // 3 = elements to copy

            // Отправляет пакет
            _comPort.Write(outPacket, 0, outPacket.Length);
            
            if (option1 != 0x06)
            {
                _sendedPacketDelivered[toId] = false;
            }
            


            return true;
            
        }

        private void Read()
        {
            while (_continue)
            {
                if (Debug)
                {
                    MessageBox.Show("in loop");
                    Debug = false;
                }

                //Если найдена сигнатура начинается обработка пакета
                if (_comPort.BytesToRead >= 2 && _comPort.ReadByte() == 0xAA && _comPort.ReadByte() == 0x55)
                {
                    // Если количество входных байтов равно или более количества байтов в Header'е
                    if (_comPort.BytesToRead >= 8)
                    {
                        // Считывает header без CRC 
                        byte[] messageHeaderWithoutHash = new byte[6];
                        _comPort.Read(messageHeaderWithoutHash, 0, 6);

                        // Сверка CRC и id
                        if (_crc16.ComputeChecksum(messageHeaderWithoutHash) == BitConverter.ToUInt16(new byte[] { (byte)_comPort.ReadByte(), (byte)_comPort.ReadByte() }, 0) && messageHeaderWithoutHash[0]==_clietnId )
                       {
                            // Вынести проверку опции в отдельный метод

                            // Если первый бит опций равен ACK 

                           MessageBox.Show("Первый бит опций равен" + Convert.ToString(messageHeaderWithoutHash[4], 16));

                            if (messageHeaderWithoutHash[4]==0x06)
                            {
                                // То сообщение клиенту от которого поступило подверждение было доставлено
                                _sendedPacketDelivered[messageHeaderWithoutHash[1]] = true;
                                // Debug message
                                MessageBox.Show("Сообщение доставлено!");
                                //return;
                                
                            }

                            else
                            {
                                // Получает значение длинны тела сообщения
                                ushort lenght =
                               BitConverter.ToUInt16(new byte[] { messageHeaderWithoutHash[2], messageHeaderWithoutHash[3] }, 0);

                                // Считывает тело сообщения 
                                byte[] messageBody = new byte[lenght];
                                _comPort.Read(messageBody, 0, lenght);

                                // Обработка полученого пакета
                                if (messageBody[0] == 0x54)
                                {
                                    byte[] messageWithOutHash = new byte[messageBody.Length - 3];
                                    Array.Copy(messageBody, 3, messageWithOutHash, 0, messageWithOutHash.Length);
                                    if (_crc16.ComputeChecksum(messageWithOutHash) == BitConverter.ToUInt16(new byte[] { messageBody[1], messageBody[2] }, 0))
                                    {
                                        MessageBox.Show("OKAY SECOND HASH IS MATCHED!");

                                        // Debug message
                                        MessageBox.Show(Encoding.UTF8.GetString(messageWithOutHash));

                                        // Выслать подверждение получения пакета
                                        SendPacket("", messageHeaderWithoutHash[1], 0x06);

                                    }
                                }
  
                            }

                           




                       }
                       else
                       {
                          // Debug message
                          MessageBox.Show("Hash or ID NOT matches!"); 
                       }
                         
                    }
                    // Надо ли оно тут?!
                    Thread.Sleep(500);
                }
            }

        }


    }
}
