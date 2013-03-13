using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatClient
{
    class ClientPort
    {
        private bool _continue = false;
        private SerialPort _comPortReader;
        private SerialPort _comPortWriter;
        private Thread _readThread;
        private Thread _writeThread;
        private Thread _fileSenderThread;
        private byte _clietnId;
        private Crc16 _crc16 = new Crc16();
        private Queue _outMessagesQueue;
        private int _outMessageQueueSize = 100;
        private ManualResetEvent _answerEvent = new ManualResetEvent(false);

        // Определяет выполняются ли cейчас операции с файлами
        private static bool _workWithFileNow = false;

        // Служит для подсчета полученных или отправленых пакетов файла.
        private byte _countOfFilePackets = 0;

        // Данные о файле для передачи
        private FileInfo _fileToTransfer;
        private bool _allowSendingFile = false;
       

        // Данные о файле для приема
        private string _receivingFileName = "";
        private long _receivingFileSize = 0;
        private string _receivingFileFullName = "";

        // Хранит CRC для последнего отправленого пакета, чтобы идентифицировать его при получении сообщения о доставке этого пакета
        private ushort _lastMessageCrc=0;

        // Хранит состояния был ли доставлен последний отправленый пакет конкретному клиенту
        private bool[] _sendedPacketDelivered = new bool[5];

        // Событие при получении данных
        //   void _comPortReader_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //   {
        //   }

        public ClientPort(string readerPortName,string writerPortName, byte id)
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

          // Создает событие получения данных
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

           _outMessagesQueue = new Queue(_outMessageQueueSize);

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

        private void FileSender(object toId)
        {
            Boolean finish = false;
            var stream = File.OpenRead(_fileToTransfer.FullName);
            var buffer = new byte[1024];

            // Количество пакетов в файле
            long totalCountOfPackets = _fileToTransfer.Length / buffer.Length;

            // Если длина файла не кратна 1024 количество пакетов больше на один 
         // if (_fileToTransfer.Length % buffer.Length != 0)
         // {
         //     totalCountOfPackets++;
         // }
         //
            // Количество уже отправленных пакетов
            long countOfSendedPackets = 0;

           // long lastPacketSize = _fileToTransfer.Length % buffer.Length;
 
            while (true)
            {
                // Получает значение сколько байт считано из файла и считывает данные в буфер
                var size = stream.Read(buffer, 0, buffer.Length);
                // Если считано 0 байт значит файл закончился
             //  if (size == 0)
             //  {
             //      break;
             //  }

                if (finish)
                {
                    break;
                }

                while (true)
                {
                    // Если буфер почти заполнен то поток ожидает 10 мс и повторяет проверку
                    if (_outMessagesQueue.Count < _outMessageQueueSize - 10)
                    {
                        // Если эот не последний пакет то отправляем весь буфер
                        if (countOfSendedPackets < totalCountOfPackets)
                        {
                            SendFilePacket(buffer, (byte) toId,_countOfFilePackets);
                            countOfSendedPackets++;
                            _countOfFilePackets++;
                        }
                        else 
                        { 
                            // Иначе отправляет только столько байт сколько было считано и этот пакет считается последним 
                            byte[] lastPacket = new byte[size];
                            Array.Copy(buffer, 0, lastPacket, 0, lastPacket.Length);
                            SendFilePacket(lastPacket, (byte)toId, _countOfFilePackets,true);
                            _workWithFileNow = false;
                            finish = true;
                        }
                        
                    break;
                    }
                        else
                            {
                                Thread.Sleep(10); 
                            }
                    
                }
                Thread.Sleep(10);
            }
        }

        private void Write()
        { 
            while (_continue)
            {
                if (_outMessagesQueue.Count > 0)
                {
                      lock (_outMessagesQueue)
                      {
                            byte[] outPacket = (byte[])_outMessagesQueue.Dequeue();
                            _answerEvent.Reset();
                          //Debug message 
                           // MessageBox.Show("Сообщений в очереди = " + _outMessagesQueue.Count.ToString());

                          // Сохраняет CRC последнего отправленого сообщения, для последующей проверки получения сообщения
                          byte[] data= new byte[outPacket.Length-10];

                           Array.Copy(outPacket,10,data,0,data.Length);

                          _lastMessageCrc = _crc16.ComputeChecksum(data);
                          
                          _comPortWriter.Write(outPacket, 0, outPacket.Length);

                          if (outPacket[6] != 0x06)
                          {
                              byte attempts = 0;

                              while (true)
                              {
                                  if (_answerEvent.WaitOne(3000, false))
                                  {
                                      // Debug message
                                      MessageBox.Show("Сообщение доставлено!");
                                      break;
                                  }
                                  else
                                  {
                                      if (++attempts > 3)
                                      {
                                          MessageBox.Show("Сообщение НЕ доставлено!");
                                          break;
                                      }
                                      // Debug message
                                      MessageBox.Show("Переотправка сообщения попытка № " + attempts);
                                      _comPortWriter.Write(outPacket, 0, outPacket.Length);
                                      
                                  }
                              }
                          }
                      }
                }
                Thread.Sleep(100);
            }
        }

        public void SendTextMessage(string message, byte toId)
        {
            // Структура пакета данных текстового сообщения 
            // | Тип пакета | Контрольная сумма данных |   Данные   |
            // |   1 байт   |          2 байта         | 0 - x байт | 

            byte option1 = 0x00;
            byte option2 = 0x00;

            // Переводит строку в массив байтов
            byte[] messageBody = Encoding.UTF8.GetBytes(message);

            // Архивирет сообщение если его длина более 1000 байт
            if (messageBody.Length>1000)
            {
                messageBody = Compressor.Zip(messageBody); // Архивация
                option2 = 0x43; // Выставляем байт опций означающий что сообщение заархивировано
            }

            // Массив байтов для отправки 
            byte[] messagePacket = new byte[messageBody.Length+3];

            // Задает тип пакета, 0x54 - текстовое сообщение
            messagePacket[0] = 0x54;

            // Вычисляет и вставляет CRC Header'а в пакет, то есть в  messagePacket[1-2]
            Array.Copy(_crc16.ComputeChecksumBytes(messageBody), 0, messagePacket, 1, 2);

            // Копирует тело сообщения в позицию после Header'а, то есть в  messagePacket[3+]
            Array.Copy(messageBody, 0, messagePacket, 3, messageBody.Length);

            AddPacketToQueue(messagePacket, toId, option1, option2);
        }

        public void SendFileTransferRequest(string filePath, byte toId)
        {
            // Структура пакета запроса на передачу файла
            // | Тип пакета | Контрольная сумма пакета |   Длина файла   | Имя файла |
            // |   1 байт   |          2 байта         |      8 байт     |  0 - 1024 |

            _fileToTransfer = new FileInfo(filePath);

            _allowSendingFile = true;

            MessageBox.Show(_fileToTransfer.Length.ToString());

            if (Encoding.UTF8.GetBytes(_fileToTransfer.Name).Length >= 1023)
            {
                MessageBox.Show("Слишком длинное имя файла");
                return;
            }

            if (!(File.Exists(filePath)))
            {
                MessageBox.Show("Файла не существует");
                return;
            }

            // Устанавливает длину конкретного пакета
            byte[] packet = new byte[11 + Encoding.UTF8.GetBytes(_fileToTransfer.Name).Length];

            // Устанавливает длину пакета для вычисления контрольной суммы
            byte[] packetWithOutHash = new byte[8 + Encoding.UTF8.GetBytes(_fileToTransfer.Name).Length];

            // Тип сообщения - запрос на передачу файла
            packet[0] = 0x52;

            // Вставляет длину файла в пакет для вычисления CRC
            Array.Copy(BitConverter.GetBytes(_fileToTransfer.Length), 0, packetWithOutHash, 0, 8);

            //Вставляет имя файла в пакет для вычисления CRC
            Array.Copy(Encoding.UTF8.GetBytes(_fileToTransfer.Name), 0, packetWithOutHash, 8, Encoding.UTF8.GetBytes(_fileToTransfer.Name).Length);

            // Вычисляет и вставляет CRC в пакет
            Array.Copy(_crc16.ComputeChecksumBytes(packetWithOutHash), 0, packet, 1, 2);

            Array.Copy(packetWithOutHash,0,packet,3,packetWithOutHash.Length);

            AddPacketToQueue(packet, toId);
        }

        private void SendFilePacket(byte[] packet, byte toId, byte packetNUmber, bool lastPacketInChain = false)
        {
            // Структура пакета файла
            // | Тип пакета | Контрольная сумма пакета | Последний пакет | Номер пакета |  Данные  |
            // |   1 байт   |          2 байта         |      1 байт     |    1 байт    |  0 - ... |

            byte option1 = 0x00;
            byte option2 = 0x00;

            byte[] messageBody = Compressor.Zip(packet); // Архивация
            
            // Массив байтов для отправки 
            byte[] messagePacket = new byte[messageBody.Length + 5];
            
            // Задает тип пакета, 0x46 - файл
            messagePacket[0] = 0x46;

            // Вычисляет и вставляет CRC Header'а в пакет, то есть в  messagePacket[1-2]
            Array.Copy(_crc16.ComputeChecksumBytes(messageBody), 0, messagePacket, 1, 2);

            // Указавает последний ли это пакет в последовательности 
            if (lastPacketInChain)
            {
                messagePacket[3] = 0x4C;
            }
            else
            {
                messagePacket[3] = 0x00;    
            }

            messagePacket[4] = packetNUmber;


            // Копирует тело сообщения в позицию после Header'а, то есть в  messagePacket[4+]
            Array.Copy(messageBody, 0, messagePacket, 5, messageBody.Length);

            AddPacketToQueue(messagePacket, toId, option1, option2);
        }

        private bool ByteArrayToFile(string _FileName, byte[] _ByteArray)
    {
        try
        {
            // Open file for reading
            FileStream _FileStream = new FileStream(_FileName, FileMode.Append, FileAccess.Write);

            // Writes a block of bytes to this stream using data from a byte array.
            _FileStream.Write(_ByteArray, 0, _ByteArray.Length);

            // close file stream
            _FileStream.Close();

            return true;
        }
        catch (Exception _Exception)
        {
            // Error
            Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
        }

    // error occured, return false
        return false;
    }

        private void AddPacketToQueue(string message, byte toId, byte option1 = 0x00, byte option2 = 0x00)
        {
        // Переводит строку в массив байтов 
            byte[] messageBody = Encoding.UTF8.GetBytes(message);

            AddPacketToQueue(messageBody, toId, option1, option2);
        }

        private bool AddPacketToQueue(byte[] messageBody, byte toId, byte option1 = 0x00, byte option2 = 0x00)
        {
            // Структура пакета
            // | Сигнатура | Получатель | Отправитель | Длинна данных |  Опции   | Контрольная сумма |   Данные   |
            // |  2 байта  |   1 байт   |   1 байт    |    2 байта    | 2 байта  |      2 байта      | 0 - x байт |
            // | 0xAA 0x55 |  

            // Контрольная сумма высчитывется по  | Получатель | Отправитель | Длинна данных |  Опции   |
            // Без учета сигнатуры                |   1 байт   |   1 байт    |    2 байта    | 2 байта  |

            // Если указанный id не предусмотрен
            if (toId > _sendedPacketDelivered.Length)
            {
                MessageBox.Show("Клиента с таким id не существует");
                return false;
            }

            //Массив всего выходного пакета, Header + тело сообщения(Data)
            byte[] outPacket = new byte[10+messageBody.Length];

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

            //Добавляем пакет в очередь на отправку
            _outMessagesQueue.Enqueue(outPacket);

            // Debug message
         //   MessageBox.Show("В Очередь добавляется пакет! Пакето в очереди = " + _outMessagesQueue.Count.ToString());

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
                    // Debug message
                    // MessageBox.Show("Сигнатура найдена");

                    // | Получатель | Отправитель | Длинна данных |  Опции   | Контрольная сумма |
                    // |   1 байт   |   1 байт    |    2 байта    | 2 байта  |      2 байта      | = 8 байт
                    // Если количество входных байтов равно или более количества байтов в Header'е без учета сигнатуры
                    if (_comPortReader.BytesToRead >= 8)
                    {
                        // Debug message
                        // MessageBox.Show("Хэдер считан");

                        // | Получатель | Отправитель | Длинна данных |  Опции   |
                        // |   1 байт   |   1 байт    |    2 байта    | 2 байта  | = 6 байт
                        // Считывает header без CRC 
                        byte[] messageHeaderWithoutHash = new byte[6];
                        _comPortReader.Read(messageHeaderWithoutHash, 0, 6);

                        // Сверка CRC и id
                        if (_crc16.ComputeChecksum(messageHeaderWithoutHash) == BitConverter.ToUInt16(new byte[] { (byte)_comPortReader.ReadByte(), (byte)_comPortReader.ReadByte() }, 0) && messageHeaderWithoutHash[0]==_clietnId )
                       {
                           // Получает значение длинны тела сообщения
                           ushort lenght =
                          BitConverter.ToUInt16(new byte[] { messageHeaderWithoutHash[2], messageHeaderWithoutHash[3] }, 0);

                           // Считывает тело сообщения 
                           byte[] messageBody = new byte[lenght];
                           _comPortReader.Read(messageBody, 0, lenght);

                   
                            // >>> Вынести проверку опций в отдельный метод <<<

                           // Обработка пакета подверждения доставки сообщения
                           // Если первый бит опций равен ACK и CRC в пакете совпала с последним отправленым
                           if (messageHeaderWithoutHash[4] == 0x06 && _lastMessageCrc == BitConverter.ToUInt16(messageBody,0))
                            {

                                // Установить что последнее сообщение было доставлено
                                _sendedPacketDelivered[messageHeaderWithoutHash[1]] = true;
                                _answerEvent.Set();
                            }

                            // Обработка пакета разрешения на передачу файла
                            // Если получено разрешение на передачу файлов
                            if (messageHeaderWithoutHash[4] == 0x41)
                            {
                                if ( _allowSendingFile)
                                {
                                    // Debug message
                                    MessageBox.Show("Запрос одобрен!!");

                                    // Начать отправку файла
                                    _allowSendingFile = false;
                                    _workWithFileNow = true;
                                    _countOfFilePackets = 0;
                                    _fileSenderThread = new Thread(FileSender);
                                    _fileSenderThread.Start(messageHeaderWithoutHash[1]);
                                }
                                
                                  // Выслать подверждение получения пакета
                                  AddPacketToQueue(BitConverter.GetBytes(_crc16.ComputeChecksum(messageBody)), messageHeaderWithoutHash[1], 0x06);
                            }
                                
                           if (messageBody.Length > 2)
                           {
                               // Обработка пакета текстового сообщения 
                               // Структура пакета текстового сообщения 
                               // | Тип пакета | Контрольная сумма данных |   Данные   |
                               // |   1 байт   |          2 байта         | 0 - x байт | 
                               if (messageBody[0] == 0x54)
                               {
                                   byte[] messageWithOutHash = new byte[messageBody.Length - 3];
                                   Array.Copy(messageBody, 3, messageWithOutHash, 0, messageWithOutHash.Length);

                                   if (_crc16.ComputeChecksum(messageWithOutHash) ==
                                       BitConverter.ToUInt16(new byte[] {messageBody[1], messageBody[2]}, 0))
                                   {
                                       // Debug message
                                       // MessageBox.Show("OKAY SECOND HASH IS MATCHED!");

                                       // Проверяет необходимоли разархивировать сообщение
                                       if (messageHeaderWithoutHash[5] == 0x43)
                                       {
                                           messageWithOutHash = Compressor.Unzip(messageWithOutHash);
                                       }

                                       // Debug message
                                       MessageBox.Show(Encoding.UTF8.GetString(messageWithOutHash));

                                       // Выслать подверждение получения пакета
                                       AddPacketToQueue(BitConverter.GetBytes(_crc16.ComputeChecksum(messageBody)), messageHeaderWithoutHash[1], 0x06);           
                                   }
                               }


                               // Обработка пакета запроса на передачу файла
                               // Структура пакета запроса на передачу файла
                               // | Тип пакета | Контрольная сумма пакета |   Длина файла   | Имя файла |
                               // |   1 байт   |          2 байта         |      8 байт     |  0 - 1024 |
                               if (messageBody[0] == 0x52)
                               {
                                   byte[] messageWithOutHash = new byte[messageBody.Length - 3];
                                   Array.Copy(messageBody, 3, messageWithOutHash, 0, messageWithOutHash.Length);


                                   if (_crc16.ComputeChecksum(messageWithOutHash) ==
                                       BitConverter.ToUInt16(new byte[] {messageBody[1], messageBody[2]}, 0))
                                   {
                                       MessageBox.Show("Совпал хеш по запросу");

                                       // Выслать подверждение получения пакета
                                       AddPacketToQueue(BitConverter.GetBytes(_crc16.ComputeChecksum(messageBody)), messageHeaderWithoutHash[1], 0x06);
                                      
                                       if (!_workWithFileNow)
                                       {
                                           _workWithFileNow = true;

                                           //Высылает разрешение на отправку файла  
                                           AddPacketToQueue("", messageHeaderWithoutHash[1], 0x41);

                                           //Обнулить счетчик пакетов файла
                                           _countOfFilePackets = 0;

                                           _receivingFileName = Encoding.UTF8.GetString(messageWithOutHash, 8,
                                                                                        messageWithOutHash.Length - 8);
                                           _receivingFileSize = BitConverter.ToInt64(messageWithOutHash, 0);

                                           // Создает файл если его еще нет
                                           CreateFile(_receivingFileName);

                                           MessageBox.Show("Размер файла = " + _receivingFileSize.ToString() + '\n' +
                                                           "Имя файла = " + _receivingFileName);
                                       }

                                   }
                               }

                               // Обработка пакета файла
                               // Структура пакета файла
                               // | Тип пакета | Контрольная сумма пакета | Последний пакет | Номер пакета |  Данные  |
                               // |   1 байт   |          2 байта         |      1 байт     |    1 байт    |  0 - ... |

                               if (messageBody[0] == 0x46 && messageBody.Length>4)
                               {
                                   byte[] messageWithOutHash = new byte[messageBody.Length - 5];
                                   Array.Copy(messageBody, 5, messageWithOutHash, 0, messageWithOutHash.Length);


                                   if (_crc16.ComputeChecksum(messageWithOutHash) ==
                                       BitConverter.ToUInt16(new byte[] { messageBody[1], messageBody[2] }, 0) && (_countOfFilePackets == messageBody[4]))
                                   {
                                       MessageBox.Show("Совпал хеш и номер пакета в пакете файла");

                                       // Выслать подверждение получения пакета
                                       AddPacketToQueue(BitConverter.GetBytes(_crc16.ComputeChecksum(messageBody)), messageHeaderWithoutHash[1], 0x06);

                                       // Разархивация данных
                                       messageWithOutHash = Compressor.Unzip(messageWithOutHash); 

                                       // Запись данных в файл
                                       ByteArrayToFile(_receivingFileFullName, messageWithOutHash);

                                       // Инкрементирует число принятых пакетов
                                       _countOfFilePackets++;

                                       // Если пакет последний в цепочке
                                       if (messageBody[3] == 0x4C)
                                       {
                                           MessageBox.Show("Файл принят! Всего пакетов в файле = " + _countOfFilePackets );
                                           _workWithFileNow = false;
                                       }



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
                }
                // Надо ли оно тут?!  Надо, но вопрос сколько именно.
                Thread.Sleep(100);
            }

        }

        public bool CreateFile (string _fileName) 
        {
            // Создает папку в "Мои документы" если ее нет
            string md = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\ChatRecivedFiles\\";
            if (Directory.Exists(md ) == false)
            {
                Directory.CreateDirectory(md);
            }

            // Создает файл если его нет
            if ((File.Exists(md+_fileName)))
            {
                MessageBox.Show("Такой файл уже существует");
                return false;
            }

            try
            {
                FileStream fs = File.Create(md + _fileName);
                _receivingFileFullName = md + _fileName;
                fs.Close();
            }
            catch (Exception _Exception)
            {
                MessageBox.Show("Exception caught in process: {0}", _Exception.ToString());
                return false;
            }

            return false;
        }




    }
}
