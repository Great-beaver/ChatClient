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
        private Queue _outMessagesQueue;
        private int _outMessageQueueSize = 200;
        private Queue _outFilePackets;
        private int _outFilePacketsSize = 200;
        public Queue InputMessageQueue;
        public int InputMessageQueueSize = 100;
        private ManualResetEvent _answerEvent = new ManualResetEvent(false);

        // Определяет сколько времение в мс потоки будут находится в состоянии сна
        private int _sleepTime = 1;

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
            _comPortReader.BaudRate = 115200;
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
           _comPortWriter.BaudRate = 115200;
           _comPortWriter.Parity = Parity.None;
           _comPortWriter.DataBits = 8;
           _comPortWriter.StopBits = StopBits.One;
           _comPortWriter.Handshake = Handshake.None;
           _comPortWriter.ReadTimeout = 500;
           _comPortWriter.WriteTimeout = 500;
           _comPortWriter.WriteBufferSize = 65000;
           _comPortWriter.ReadBufferSize = 65000;

           _outMessagesQueue = new Queue(_outMessageQueueSize);

           _outFilePackets = new Queue(_outFilePacketsSize);

            InputMessageQueue = new Queue(InputMessageQueueSize);

            

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
            // TO DO: Разобратся с длинной пакета, допустима ли такая длинна пакета?
            var buffer = new byte[1024*50];

            // Количество пакетов в файле
            long totalCountOfPackets = _fileToTransfer.Length / buffer.Length;

            // Количество уже отправленных пакетов
            long countOfSendedPackets = 0;
 
            while (true)
            {
                // Получает значение сколько байт считано из файла и считывает данные в буфер
                var size = stream.Read(buffer, 0, buffer.Length);

                // Завершает цикл если установлен соотвествующий флаг
                if (finish)
                {
                    break;
                }

                while (true)
                {
                    // Если буфер почти заполнен то поток ожидает 10 мс и повторяет проверку
                    if (QueueCount(_outFilePackets) < _outFilePacketsSize)
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
                                Thread.Sleep(_sleepTime); 
                            }
                    
                }
                Thread.Sleep(_sleepTime);
            }
        }

        private void Write()
        {
            byte[] outPacket = new byte[0];
            while (_continue)
            {
                Thread.Sleep(_sleepTime);

                // Определяет есть ли пакеты в очереди
                if (QueueCount(_outMessagesQueue) > 0)
                {
                    // Блокировка очередни на время извлечения пакета
                    lock (_outMessagesQueue)
                    {
                        outPacket = (byte[]) _outMessagesQueue.Dequeue();
                    }

                }
                else
                {
                    if (QueueCount(_outFilePackets) > 0)
                    {
                        // Блокировка очередни на время извлечения пакета
                        lock (_outFilePackets)
                        {
                            outPacket = (byte[])_outFilePackets.Dequeue();
                        }

                    }
                    else
                    {
                        continue;
                    }
                }
                    

                _answerEvent.Reset();

                          // Сохраняет CRC последнего отправленого сообщения, для последующей проверки получения сообщения
                          byte[] data= new byte[outPacket.Length-10];

                           Array.Copy(outPacket,10,data,0,data.Length);

                          _lastMessageCrc = Crc16.ComputeChecksum(data);

                          lock (_comPortWriter)
                          {
                                _comPortWriter.Write(outPacket, 0, outPacket.Length);
                          }
                    
                              byte attempts = 0;

                              while (true)
                              {
                                  if (_answerEvent.WaitOne(3000, false))
                                  {
                                      if (PacketType(outPacket)=="Text")
                                      {
                                          InputMessageQueue.Enqueue("Сообщение доставлено!");  
                                      }
                                      break;
                                  }
                                  else
                                  {
                                      if (++attempts > 3)
                                      {
                                          InputMessageQueue.Enqueue("Сообщение НЕ доставлено!");
                                          break;
                                      }

                                            // Debug message
                                            MessageBox.Show("Переотправка сообщения попытка № " + attempts);

                                            lock (_comPortWriter)
                                            {
                                                _comPortWriter.Write(outPacket, 0, outPacket.Length);
                                            }
                                  }
                              }
                          
                      
                
                
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
            Array.Copy(Crc16.ComputeChecksumBytes(messageBody), 0, messagePacket, 1, 2);

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
            Array.Copy(Crc16.ComputeChecksumBytes(packetWithOutHash), 0, packet, 1, 2);

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
            Array.Copy(Crc16.ComputeChecksumBytes(messageBody), 0, messagePacket, 1, 2);

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

        private bool ByteArrayToFile(string fileName, byte[] byteArray)
    {
        try
        {
            // Открывает файл в режими для записи в конец файла
            FileStream _FileStream = new FileStream(fileName, FileMode.Append, FileAccess.Write);

            // Записывает блок байтов в поток и следовательно в файл
            _FileStream.Write(byteArray, 0, byteArray.Length);

            // Закрывает поток
            _FileStream.Close();

            return true;
        }
        catch (Exception _Exception)
        {
            // Ошибка
            Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
        }

    // В случае ошибки возвращает false
        return false;
    }

        private void AddPacketToQueue(string message, byte toId, byte option1 = 0x00, byte option2 = 0x00)
        {
        // Переводит строку в массив байтов 
            byte[] messageBody = Encoding.UTF8.GetBytes(message);

            AddPacketToQueue(messageBody, toId, option1, option2);
        }

        private bool AddPacketToQueue(byte[] messageBody, byte toId, byte option1 = 0x00, byte option2 = 0x00, bool sendPacketImmediately = false)
        {
            // WTF!?
            // Если указанный id не предусмотрен
            if (toId > _sendedPacketDelivered.Length)
            {
                MessageBox.Show("Клиента с таким id не существует");
                return false;
            }

            Packet packet = new Packet(toId,_clietnId,option1,option2,messageBody);

            if (sendPacketImmediately)
            {
                lock (_comPortWriter)
                {
                    _comPortWriter.Write(packet.ToByte(), 0, packet.ToByte().Length);
                }
                return true;
            }

            if (PacketType(packet.ToByte())=="File")
            {
                lock (_outFilePackets)
                {
                    _outFilePackets.Enqueue(packet.ToByte());
                }
                return true;
            }

          

                //Добавляем пакет в очередь на отправку
                lock (_outMessagesQueue)
                {
                    _outMessagesQueue.Enqueue(packet.ToByte());
                }
            


            return true;
        }

        private void Read()
        {

            while (_continue)
            {
                // Структура заголовка пакета
                // | Сигнатура | Получатель | Отправитель | Длинна данных |  Опции   | Контрольная сумма |   Данные   | 
                // |  2 байта  |   1 байт   |   1 байт    |    2 байта    | 2 байта  |      2 байта      | 0 - x байт |

                //Если найдена сигнатура | 0xAA 0x55 | начинается обработка пакета
                if (_comPortReader.BytesToRead >= 2 && _comPortReader.ReadByte() == 0xAA && _comPortReader.ReadByte() == 0x55)
                {
                    // | Получатель | Отправитель | Длинна данных |  Опции   | Контрольная сумма |
                    // |   1 байт   |   1 байт    |    2 байта    | 2 байта  |      2 байта      | = 8 байт
                    // Если количество входных байтов равно или более количества байтов в Header'е без учета сигнатуры
                    if (_comPortReader.BytesToRead >= 8)
                    {
                        // | Получатель | Отправитель | Длинна данных |  Опции   |
                        // |   1 байт   |   1 байт    |    2 байта    | 2 байта  | = 6 байт
                        // Считывает header без CRC 
                        byte[] messageHeaderWithoutHash = new byte[6];
                        _comPortReader.Read(messageHeaderWithoutHash, 0, 6);

                        //Функция разбора пакета
                        ParsePacket(messageHeaderWithoutHash);
                         
                    } 
                }
                // Если данных нет, ожидать в течении _sleepTime
                Thread.Sleep(_sleepTime);
            }

        }

        private void ParsePacket(byte[] packetHeaderWithoutHash)
        {

            // Сверка CRC и id
            if (Crc16.ComputeChecksum(packetHeaderWithoutHash) == BitConverter.ToUInt16(new byte[] { (byte)_comPortReader.ReadByte(), (byte)_comPortReader.ReadByte() }, 0) && packetHeaderWithoutHash[0] == _clietnId)
            {
                // Получает значение длинны тела сообщения
                ushort lenght =
               BitConverter.ToUInt16(new byte[] { packetHeaderWithoutHash[2], packetHeaderWithoutHash[3] }, 0);

                // Считывает тело сообщения 
                byte[] messageBody = new byte[lenght];
                _comPortReader.Read(messageBody, 0, lenght);


                // >>> Вынести проверку опций в отдельный метод <<<

                // Обработка пакета подверждения доставки сообщения
                // Если первый бит опций равен ACK и CRC в пакете совпала с последним отправленым
                if (packetHeaderWithoutHash[4] == 0x06 && _lastMessageCrc == BitConverter.ToUInt16(messageBody, 0))
                {

                    // Установить что последнее сообщение было доставлено
                    _sendedPacketDelivered[packetHeaderWithoutHash[1]] = true;
                    _answerEvent.Set();
                }

                // Обработка пакета разрешения на передачу файла
                // Если получено разрешение на передачу файлов
                if (packetHeaderWithoutHash[4] == 0x41)
                {
                    if (_allowSendingFile)
                    {
#if DEBUG
                                    // Debug message
                                    MessageBox.Show("Запрос одобрен!!");
#endif
                        // Начать отправку файла
                        _allowSendingFile = false;
                        _workWithFileNow = true;
                        _countOfFilePackets = 0;
                        _fileSenderThread = new Thread(FileSender);
                        _fileSenderThread.Start(packetHeaderWithoutHash[1]);
                    }

                    // Выслать подверждение получения пакета
                    AddPacketToQueue(BitConverter.GetBytes(Crc16.ComputeChecksum(messageBody)), packetHeaderWithoutHash[1], 0x06, 0x00, true);
                }

                // Защита от случая если в пакете меньше данных чем необходимо для обработки
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

                        if (Crc16.ComputeChecksum(messageWithOutHash) ==
                            BitConverter.ToUInt16(new byte[] { messageBody[1], messageBody[2] }, 0))
                        {
                            // Проверяет необходимоли разархивировать сообщение
                            if (packetHeaderWithoutHash[5] == 0x43)
                            {
                                messageWithOutHash = Compressor.Unzip(messageWithOutHash);
                            }

                            // Debug message
                            //MessageBox.Show(Encoding.UTF8.GetString(messageWithOutHash));
                            lock (InputMessageQueue)
                            {
                                InputMessageQueue.Enqueue(Encoding.UTF8.GetString(messageWithOutHash)); 
                            }
                            

                            // Выслать подверждение получения пакета
                            AddPacketToQueue(BitConverter.GetBytes(Crc16.ComputeChecksum(messageBody)), packetHeaderWithoutHash[1], 0x06,0x00,true);
                        }
                        else
                        {
                            //Debug message
                            MessageBox.Show("Ткстовый пакет: хеш не совпал");
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


                        if (Crc16.ComputeChecksum(messageWithOutHash) ==
                            BitConverter.ToUInt16(new byte[] { messageBody[1], messageBody[2] }, 0))
                        {
                            // Выслать подверждение получения пакета
                            AddPacketToQueue(BitConverter.GetBytes(Crc16.ComputeChecksum(messageBody)), packetHeaderWithoutHash[1], 0x06, 0x00, true);

                            if (!_workWithFileNow)
                            {
                                _workWithFileNow = true;

                                //Высылает разрешение на отправку файла  
                                AddPacketToQueue("", packetHeaderWithoutHash[1], 0x41);

                                //Обнулить счетчик пакетов файла
                                _countOfFilePackets = 0;

                                _receivingFileName = Encoding.UTF8.GetString(messageWithOutHash, 8,
                                                                             messageWithOutHash.Length - 8);
                                _receivingFileSize = BitConverter.ToInt64(messageWithOutHash, 0);

                                // Создает файл если его еще нет
                                CreateFile(_receivingFileName);
#if DEBUG
MessageBox.Show("Размер файла = " + _receivingFileSize.ToString() + '\n' +
                "Имя файла = " + _receivingFileName);
#endif
                            }

                        }
                    }

                    // Обработка пакета файла
                    // Структура пакета файла
                    // | Тип пакета | Контрольная сумма пакета | Последний пакет | Номер пакета |  Данные  |
                    // |   1 байт   |          2 байта         |      1 байт     |    1 байт    |  0 - ... |

                    if (messageBody[0] == 0x46 && messageBody.Length > 4)
                    {
                        byte[] messageWithOutHash = new byte[messageBody.Length - 5];
                        Array.Copy(messageBody, 5, messageWithOutHash, 0, messageWithOutHash.Length);


                        if (Crc16.ComputeChecksum(messageWithOutHash) ==
                            BitConverter.ToUInt16(new byte[] { messageBody[1], messageBody[2] }, 0) && (_countOfFilePackets == messageBody[4]))
                        {
                            // Выслать подверждение получения пакета
                            AddPacketToQueue(BitConverter.GetBytes(Crc16.ComputeChecksum(messageBody)), packetHeaderWithoutHash[1], 0x06 ,0x00, true);

                            // Разархивация данных
                            messageWithOutHash = Compressor.Unzip(messageWithOutHash);

                            // Запись данных в файл
                            ByteArrayToFile(_receivingFileFullName, messageWithOutHash);

                            // Инкрементирует число принятых пакетов
                            _countOfFilePackets++;

                            // Если пакет последний в цепочке
                            if (messageBody[3] == 0x4C)
                            {
                                MessageBox.Show("Файл принят! Всего пакетов в файле = " + _countOfFilePackets);
                                _workWithFileNow = false;
                            }
                        }
                        else
                        {
                            //Debug messages
                            if (Crc16.ComputeChecksum(messageWithOutHash) !=
                            BitConverter.ToUInt16(new byte[] { messageBody[1], messageBody[2] }, 0))
                            {
                                MessageBox.Show("Пакет файла: не совпал хеш");
                            }
                           
                            if (_countOfFilePackets != messageBody[4])
                            {
                                MessageBox.Show("Пакет файла: не совпал номер пакета, принятый номер " + messageBody[4] + "сохраненый номер " + _countOfFilePackets);
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

        public bool CreateFile (string fileName) 
        {
            // Создает папку в "Мои документы" если ее нет
            string md = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\ChatRecivedFiles\\";
            if (Directory.Exists(md ) == false)
            {
                Directory.CreateDirectory(md);
            }

            // Проверяет наличие файла
            if ((File.Exists(md+fileName)))
            {
                // Если файл существует то предлагает его перезаписать
                DialogResult dialogResult = MessageBox.Show("Файл " + fileName  + " уже существует, перезаписать его?", "Перезаписать файл?", MessageBoxButtons.YesNo);

                // В случае отказа прекращает фанкцию
                if (dialogResult == DialogResult.No)
                {

                    // TO DO: Добавить код отмены приема файла
                    return false;
                }               
            }

            try
            {
                FileStream fs = File.Create(md + fileName);
                _receivingFileFullName = md + fileName;
                fs.Close();
            }
            catch (Exception _Exception)
            {
                MessageBox.Show("Exception caught in process: {0}", _Exception.ToString());
                return false;
            }

            return false;
        }

        private string PacketType(byte[] packet)
        {
            if (packet.Length<11)
            {
                return "Error";
            }

            if (packet[10] == 0x54)
            {
                return "Text";
            }

            if (packet[10] == 0x46)
            {
                return "File";
            }

            return "Unknown";



        }

        private int QueueCount (Queue queue)
        {
            lock (queue)
            {
                return queue.Count;
            }
            
        }


    }
}
