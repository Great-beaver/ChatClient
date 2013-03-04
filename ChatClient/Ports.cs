using System;
using System.Collections;
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
        private SerialPort _comPortReader;
        private SerialPort _comPortWriter;
        private Thread _readThread;
        private Thread _writeThread;
        private byte _clietnId;
        private byte[] _readBufferHeader = new byte[6];
        private Crc16 _crc16 = new Crc16();
        byte[] _reciveMessageLenght;
        private Queue _outMessagesQueue;

        public bool Debug = false;

        // Хранит состояния был ли доставлен последний отправленый пакет конкретному клиенту
        private bool[] _sendedPacketDelivered = new bool[5];
        
        public Ports(string readerPortName,string writerPortName, byte id)
        {
            _comPortReader = new SerialPort();
            _comPortReader.PortName = readerPortName;
            _comPortReader.BaudRate = 9600;
            _comPortReader.Parity = Parity.None;
            _comPortReader.DataBits = 8;
            _comPortReader.StopBits = StopBits.One;
            _comPortReader.Handshake = Handshake.None;
            _comPortReader.ReadTimeout = 500;
            _comPortReader.WriteTimeout = 500;
            _comPortReader.WriteBufferSize = 65000;
            _comPortReader.ReadBufferSize = 65000;

          //  _comPortReader.DataReceived += new SerialDataReceivedEventHandler(_comPortReader_DataReceived);

           _comPortWriter = new SerialPort();
           _comPortWriter.PortName = writerPortName;
           _comPortWriter.BaudRate = 9600;
           _comPortWriter.Parity = Parity.None;
           _comPortWriter.DataBits = 8;
           _comPortWriter.StopBits = StopBits.One;
           _comPortWriter.Handshake = Handshake.None;
           _comPortWriter.ReadTimeout = 500;
           _comPortWriter.WriteTimeout = 500;
           _comPortWriter.WriteBufferSize = 65000;
           _comPortWriter.ReadBufferSize = 65000;

           _outMessagesQueue = new Queue(100);

            // Пакет разрешается отправить только если значение равно true
            // При отправке пакета значение устанавливается в false
            // Пакет должен стать true после получения подтверждения о доставке или отмены доставки
            _sendedPacketDelivered[0] = true;
            _sendedPacketDelivered[1] = true;
            _sendedPacketDelivered[2] = true;
            _sendedPacketDelivered[3] = true;
            _sendedPacketDelivered[4] = true;

            // Особая уличная кодировка для правильной отправки байтов чьё значение больше 127-ми
            _comPortReader.Encoding = Encoding.GetEncoding(28591);

            _comPortWriter.Encoding = Encoding.GetEncoding(28591);

            _clietnId = id;

            _readThread = new Thread(Read);
            _writeThread = new Thread(Write);

            _comPortReader.Open();
            _comPortWriter.Open();

            _continue = true;
            _readThread.Start();
            _writeThread.Start();
        }

    //   void _comPortReader_DataReceived(object sender, SerialDataReceivedEventArgs e)
    //   {
    //      // MessageBox.Show("Да да, что то пришло");
    //       Read();
    //   }

        public void Write()
        {

            while (_continue)
            {
                if (_outMessagesQueue.Count > 0)
                {
                      lock (_outMessagesQueue)
                      {
                            byte[] outPacket = (byte[])_outMessagesQueue.Dequeue();
                            _comPortWriter.Write(outPacket, 0, outPacket.Length); 
                      }
                }
            }
        }

        public void SendTextMessage(string message, byte toId)
        {
            // Структура пакета данных текстового сообщения 
            // | Тип пакета | Контрольная сумма данных |   Данные   |
            // |   1 байт   |          2 байта         | 0 - x байт | 

            // Переводит строку в массив байтов
            byte[] messageBody = Encoding.UTF8.GetBytes(message);

            // Массив байтов для отправки
            byte[] messagePacket = new byte[messageBody.Length+3];

            MessageBox.Show("Длина тела отправленного сообщения = " + messageBody.Length);

            // Задает тип пакета, 0x54 - текстовое сообщение
            messagePacket[0] = 0x54;

            // Вычисляет и вставляет CRC Header'а в пакет, то есть в  messagePacket[1-2]
            Array.Copy(_crc16.ComputeChecksumBytes(messageBody), 0, messagePacket, 1, 2);

            // Копирует тело сообщения в позицию после Header'а, то есть в  messagePacket[3+]
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
            // Структура пакета
            // | Сигнатура | Получатель | Отправитель | Длинна данных |  Опции   | Контрольная сумма |   Данные   |
            // |  2 байта  |   1 байт   |   1 байт    |    2 байта    | 2 байта  |      2 байта      | 0 - x байт |
            // | 0xAA 0x55 |  

            // Контрольная сумма высчитывется по  | Получатель | Отправитель | Длинна данных |  Опции   |
            // Без учета сигнатуры                |   1 байт   |   1 байт    |    2 байта    | 2 байта  |

            // Если ожидается доставка предыдущего пакета то сообщение не будет отправлено
            // Но если сообщение является подверждением доставки то оно будет отправлено
           if (!_sendedPacketDelivered[toId] && option1 != 0x06)
           {
               // Debug message
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
            // | Получатель | Отправитель | Длинна данных |  Опции   |
            // |   1 байт   |   1 байт    |    2 байта    | 2 байта  | = 6 байт
            byte[] packetWithOutHash = new byte[6];

            //Сигнатура header'а
            outPacket[0] = 0xAA;
            outPacket[1] = 0x55;

            //Адрес получателя
            packetWithOutHash[0] = toId;

            //Адрес отправителя
            packetWithOutHash[1] = _clietnId;

            // Копирует тело сообщения в позицию после Header'а 
            Array.Copy(messageBody,0,outPacket,10,messageBody.Length);

            // Вставляет длинну тела сообщения в Header'а, то есть в packetWithOutHash[2-3] 
            Array.Copy(BitConverter.GetBytes(((short)messageBody.Length)), 0, packetWithOutHash, 2, 2);

            // Выставляет опции в Header
            packetWithOutHash[4] = option1;
            packetWithOutHash[5] = option2;

            // Вставляет header без CRC и сигнатуры в начало пакета после сигнатуры 
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


            //Добавляем пакет в оячередь на отправку
            _outMessagesQueue.Enqueue(outPacket);

            // Отправляет пакет
          //  _comPortWriter.Write(outPacket, 0, outPacket.Length);
            
            // Если отправляемый пакет это acknowledge то не ждать отчета о его доставки 
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
                // Структура пакета
                // | Сигнатура | Получатель | Отправитель | Длинна данных |  Опции   | Контрольная сумма |   Данные   | 
                // |  2 байта  |   1 байт   |   1 байт    |    2 байта    | 2 байта  |      2 байта      | 0 - x байт |

                //Если найдена сигнатура | 0xAA 0x55 | начинается обработка пакета
                if (_comPortReader.BytesToRead >= 2 && _comPortReader.ReadByte() == 0xAA && _comPortReader.ReadByte() == 0x55)
                {
                    MessageBox.Show("Сигнатура найдена");

                    // | Получатель | Отправитель | Длинна данных |  Опции   | Контрольная сумма |
                    // |   1 байт   |   1 байт    |    2 байта    | 2 байта  |      2 байта      | = 8 байт
                    // Если количество входных байтов равно или более количества байтов в Header'е без учета сигнатуры
                    if (_comPortReader.BytesToRead >= 8)
                    {
                        MessageBox.Show("Хэдер считан");

                        // | Получатель | Отправитель | Длинна данных |  Опции   |
                        // |   1 байт   |   1 байт    |    2 байта    | 2 байта  | = 6 байт
                        // Считывает header без CRC 
                        byte[] messageHeaderWithoutHash = new byte[6];
                        _comPortReader.Read(messageHeaderWithoutHash, 0, 6);

                        // Сверка CRC и id
                        if (_crc16.ComputeChecksum(messageHeaderWithoutHash) == BitConverter.ToUInt16(new byte[] { (byte)_comPortReader.ReadByte(), (byte)_comPortReader.ReadByte() }, 0) && messageHeaderWithoutHash[0]==_clietnId )
                       {
                            
                           MessageBox.Show("Первый бит опций равен" + Convert.ToString(messageHeaderWithoutHash[4], 16));
                    // >>> Вынести проверку опций в отдельный метод <<<
                           // Если первый бит опций равен ACK 
                            if (messageHeaderWithoutHash[4]==0x06)
                            {
                                // Отправить подверждение клиенту от которого поступил пакет 
                                _sendedPacketDelivered[messageHeaderWithoutHash[1]] = true;
                                // Debug message
                                MessageBox.Show("Сообщение доставлено!");
                            }

                            else
                            {
                                // Получает значение длинны тела сообщения
                                ushort lenght =
                               BitConverter.ToUInt16(new byte[] { messageHeaderWithoutHash[2], messageHeaderWithoutHash[3] }, 0);

                                // Считывает тело сообщения 
                                byte[] messageBody = new byte[lenght];
                                _comPortReader.Read(messageBody, 0, lenght);

                                // Обработка полученого пакета
                                // Структура пакета данных текстового сообщения 
                                // | Тип пакета | Контрольная сумма данных |   Данные   |
                                // |   1 байт   |          2 байта         | 0 - x байт | 
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
                    Thread.Sleep(5000);
                }
            }

        }


    }
}
