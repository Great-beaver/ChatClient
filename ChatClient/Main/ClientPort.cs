using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ChatClient.Main.Packet;

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
        private Thread _waitForFilePacketThread;
        private byte _clietnId;
        private Queue _outMessagesQueue;
        private int _outMessageQueueSize = 200;
        private Queue _outFilePacketsQueue;
        private int _outFilePacketsQueueSize = 200;
        public Queue InputMessageQueue;
        public int InputMessageQueueSize = 100;
        // Событие при получении Acknowledge
        private ManualResetEvent _answerEvent = new ManualResetEvent(false);

        // Событие при получении пакета файла
        private ManualResetEvent _waitForFilePacketEvent = new ManualResetEvent(false);
        // Таймаут ожидания пакета файла в мс
        private int _waitForFilePacketTimeout = 4000;

        // Определяет выполняются ли cейчас операции с файлами
        // private static bool _workWithFileNow = false;

        // Отражает состояние принимается ли или передается ли файл
        private bool _isRecivingFile = false;
        private bool _isSendingFile = false;
        
        // Определяет сколько времение в мс потоки будут находится в состоянии сна
        private int _sleepTime = 1;
      
        // Служит для подсчета полученных или отправленых пакетов файла.
        private byte _countOfFilePackets = 0;

        // Данные о файле для передачи
        private FileInfo _fileToTransfer;
        // Получатель файла
        private byte _fileRecipient;
        
        // Разрешает передачу файла при приему разрешения на передачу. Устанавливается если был отправлен запрос на передачу файла
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

           _outFilePacketsQueue = new Queue(_outFilePacketsQueueSize);

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

        private void WaitForFilePacket()
        {
            // Пока стоит флаг приема файла
            while (_isRecivingFile)
            {
                // Обнулить событие
                _waitForFilePacketEvent.Reset();
                // Если вышел таймаут получения пакета файла
                if (_waitForFilePacketEvent.WaitOne(_waitForFilePacketTimeout, false))
                {
                    // Так как метод WaitOne не возвращает значение false, необходимо встваить пустой блок кода если сингнал получен
                    // Возможно есть смысл вставить сюда continue
                }
                else
                {
                    // Проверка принимается ли еще файл так как за время таймаута ситуация могла изменится
                    if (_isRecivingFile)
                    {
#if DEBUG
                        MessageBox.Show("Все пропало, файла не будет.");
#endif
                        CancelRecivingFile();
                    }

                    break;
                }
                Thread.Sleep(_sleepTime);
            }
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

            // Отчищает очередь пакетов файла на передачу 
            lock (_outFilePacketsQueue)
            {
                _outFilePacketsQueue.Clear();
            }
 
            while (true && _isSendingFile)
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
                    if (QueueCount(_outFilePacketsQueue) < _outFilePacketsQueueSize)
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
                            //_workWithFileNow = false;

                            // TO DO: Убрать эту строку!
                            // _isSendingFile = false;
                            //
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

#if DEBUG
            MessageBox.Show("Поток отправки файла завершен");
#endif
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
                    // Определяет есть ли пакеты файлов в очереди и разрешена передача файла 
                    if (QueueCount(_outFilePacketsQueue) > 0 && _isSendingFile)
                    {
                        // Блокировка очередни на время извлечения пакета
                        lock (_outFilePacketsQueue)
                        {
                            outPacket = (byte[])_outFilePacketsQueue.Dequeue();
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
                                      if (PacketType(outPacket)=="File" && !_isSendingFile)
                                      {
                                          break;
                                      }

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
            // | Тип пакета |   Данные   |
            // |   1 байт   | 0 - x байт | 

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
            byte[] messagePacket = new byte[messageBody.Length+1];

            // Задает тип пакета, 0x54 - текстовое сообщение
            messagePacket[0] = 0x54;

            // Копирует тело сообщения в позицию после Header'а, то есть в  messagePacket[1+]
            Array.Copy(messageBody, 0, messagePacket, 1, messageBody.Length);

            AddPacketToQueue(messagePacket, toId, option1, option2);
        }

        public void SendFileTransferRequest(string filePath, byte toId)
        {
            // TO DO: Отправлять запрос только если не идет работа с файлом

            // Структура пакета запроса на передачу файла
            // | Тип пакета |   Длина файла   | Имя файла |
            // |   1 байт   |      8 байт     |  0 - 1024 |

            _fileToTransfer = new FileInfo(filePath);

            _allowSendingFile = true;
#if DEBUG
            MessageBox.Show(_fileToTransfer.Length.ToString());
#endif
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
            byte[] packet = new byte[9 + Encoding.UTF8.GetBytes(_fileToTransfer.Name).Length];


            // Тип сообщения - запрос на передачу файла
            packet[0] = 0x52;

            // Вставляет длину файла в пакет
            Array.Copy(BitConverter.GetBytes(_fileToTransfer.Length), 0, packet, 1, 8);

            //Вставляет имя файла в пакет
            Array.Copy(Encoding.UTF8.GetBytes(_fileToTransfer.Name), 0, packet, 9, Encoding.UTF8.GetBytes(_fileToTransfer.Name).Length);

            AddPacketToQueue(packet, toId);
        }

        private void SendFilePacket(byte[] packet, byte toId, byte packetNUmber, bool lastPacketInChain = false)
        {
            // Структура пакета файла
            // | Тип пакета | Последний пакет | Номер пакета |  Данные  |
            // |   1 байт   |      1 байт     |    1 байт    |  0 - ... |

            byte option1 = 0x00;
            byte option2 = 0x00;

            byte[] messageBody = Compressor.Zip(packet); // Архивация
            
            // Массив байтов для отправки 
            byte[] Packet = new byte[messageBody.Length + 3];
            
            // Задает тип пакета, 0x46 - файл
            Packet[0] = 0x46;

            // Указавает последний ли это пакет в последовательности 
            if (lastPacketInChain)
            {
                Packet[1] = 0x4C;
            }
            else
            {
                Packet[1] = 0x00;    
            }

            Packet[2] = packetNUmber;

            // Копирует тело сообщения в позицию после Header'а, то есть в  messagePacket[4+]
            Array.Copy(messageBody, 0, Packet, 3, messageBody.Length);

            AddPacketToQueue(Packet, toId, option1, option2);
        }

        private bool AddPacketToQueue(byte[] messageBody, byte toId, byte option1 = 0x00, byte option2 = 0x00, bool sendPacketImmediately = false)
        {
            Packet packet = new Packet(toId,_clietnId,option1,option2,messageBody);

            // TO DO: Поправить логику условий
            // Валидация данных

            if (sendPacketImmediately)
            {
                lock (_comPortWriter)
                {
                    _comPortWriter.Write(packet.ToByte(), 0, packet.ToByte().Length);
                }
                return true;
            }

            if (packet.Data.Type == "FileData")
            {
                lock (_outFilePacketsQueue)
                {
                    _outFilePacketsQueue.Enqueue(packet.ToByte());
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
                //Если найдена сигнатура | 0xAA 0x55 | начинается обработка пакета
                if (_comPortReader.BytesToRead >= 2 && _comPortReader.ReadByte() == 0xAA && _comPortReader.ReadByte() == 0x55)
                {
                    // Если количество входных байтов равно или более количества байтов в Header'е без учета сигнатуры
                    if (_comPortReader.BytesToRead >= 8)
                    {
                        // Считывание данных для создания пакета
                        // Здесь важен строгий порядок считывания байтов, точно как в пакете.
                        byte recipient = (byte)_comPortReader.ReadByte();
                        byte sender = (byte)_comPortReader.ReadByte();
                        ushort dataLenght = BitConverter.ToUInt16(
                                new byte[] {(byte) _comPortReader.ReadByte(), (byte) _comPortReader.ReadByte()}, 0);
                        byte option1 = (byte)_comPortReader.ReadByte();
                        byte option2 = (byte)_comPortReader.ReadByte();
                        ushort crc = BitConverter.ToUInt16(
                                new byte[] { (byte)_comPortReader.ReadByte(), (byte)_comPortReader.ReadByte() }, 0);

                        byte[] data = new byte[dataLenght];
                        _comPortReader.Read(data, 0, dataLenght);

                        Packet packet = new Packet(recipient, sender, option1, option2, data);

                        // Проверка crc и id клиента, то есть предназначен ли этот пакет этому клиенту.
                        if (packet.Crc == crc && packet.Recipient == _clietnId)
                        {
                            //Функция разбора пакета
                            ParsePacket(packet);
                        }
                        else
                        {
                            MessageBox.Show("Hash or ID NOT matches!");    
                        } 
                    } 
                }
                // Если данных нет, ожидать в течении _sleepTime
                Thread.Sleep(_sleepTime);
            }
        }

        private void SendFileTransferCancel(byte toId)
        {
            AddPacketToQueue(new byte[] { 0x00 }, toId, 0x18);

        }

        private void SendAcknowledge(Packet packet)
        {
            AddPacketToQueue(BitConverter.GetBytes(Crc16.ComputeChecksum(packet.ByteData)), packet.Sender, 0x06, 0x00, true);
        }

        private void SendFileTransferCompleted(byte toId)
        {
            AddPacketToQueue(new byte[] { 0x00 }, toId, 0x04);
        }

        private void ParsePacket(Packet packet)
        {
                // >>> Вынести проверку опций в отдельный метод <<<

                // Обработка пакета подверждения доставки сообщения
                // Если первый бит опций равен ACK и CRC в пакете совпала с последним отправленым
                if (packet.Option1String == "ACK" && _lastMessageCrc == BitConverter.ToUInt16(packet.ByteData, 0))
                {
                    // Установить что последнее сообщение было доставлено
                    _sendedPacketDelivered[packet.Sender] = true;
                    _answerEvent.Set();
                   return;
                }

                // Обработка пакета разрешения на передачу файла
                // Если получено разрешение на передачу файлов
                if (packet.Option1String == "FileTransferAllowed")
                {
                    if (_allowSendingFile)
                    {
#if DEBUG
                                    // Debug message
                                    MessageBox.Show("Запрос одобрен!!");
#endif
                        // Начать отправку файла

                                    // Отчищает очередь пакетов файла на передачу 
                                    lock (_outFilePacketsQueue)
                                    {
                                        _outFilePacketsQueue.Clear();
                                    }

                        // Запрещает отправлять файла на последующие запросы до завершения передачи
                        _allowSendingFile = false;
                       //_workWithFileNow = true;
                        // Устанавливает что идет передача файла
                        _isSendingFile = true;
                        // Обнуляет счетчик пакетов
                        _countOfFilePackets = 0;
                        // Устанавливает кому будет передаваться файл
                        _fileRecipient = packet.Sender;
                        // Запускает поток для упаковки файла в пакеты и добавления их в очередь
                        _fileSenderThread = new Thread(FileSender);
                        _fileSenderThread.Start(packet.Sender);
                    }

                    // Выслать подверждение получения пакета
                    
                   SendAcknowledge(packet);
                    return;
                }
            
            // Если получен пакет отмены или отказа передачи файла
            if (packet.Option1String == "FileTransferDenied")
            {
                // Если принимался файл
                if (_isRecivingFile)
                {
#if DEBUG
                    // Debug message
                    MessageBox.Show("Отправитель отменил передачу файла");
#endif
                    CancelRecivingFile();
                    // Выслать подверждение получения пакета
                    SendAcknowledge(packet);
                    return;
                }

                if (_isSendingFile)
                {
#if DEBUG
                    // Debug message
                    MessageBox.Show("Получатель отменил передачу");
#endif
                    CancelSendingFile();
                    // Выслать подверждение получения пакета
                    SendAcknowledge(packet);
                    return;
                }
                
                if (_allowSendingFile)
                {
                    _allowSendingFile = false;
#if DEBUG
                    // Debug message
                    MessageBox.Show("В передаче файла отказано");
#endif        
                }
            
                // Выслать подверждение получения пакета
                SendAcknowledge(packet);
                return;
            }

            if (packet.Option1String == "FileTransferCompleted")
            {
                _isSendingFile = false;
#if DEBUG
                // Debug message
                MessageBox.Show("Файл доставлен");
#endif      
                SendAcknowledge(packet);
                return;
            }

            switch (packet.Data.Type)
            {

                // Обработка пакета текстового сообщения 
                case "Text" : 
                    {
                        // Определяет необходимость разархивирования
                        if (packet.Option2String == "Compressed")
                        {
                            packet.Data.Content = Compressor.Unzip(packet.Data.Content);
                        }

                        lock (InputMessageQueue)
                        {
                            InputMessageQueue.Enqueue(Encoding.UTF8.GetString(packet.Data.Content));
                        }
                        // Выслать подверждение получения пакета
                        AddPacketToQueue(BitConverter.GetBytes(Crc16.ComputeChecksum(packet.ByteData)), packet.Sender, 0x06, 0x00, true);
                    }
                    break;

                // Обработка пакета запроса на передачу файла
                case "FileRequest":
                    {
                        // Выслать подверждение получения пакета
                       SendAcknowledge(packet);

                        //if (!_workWithFileNow)
                        // Если не принимается и не передается файл
                        if (!_isSendingFile && !_isRecivingFile)
                        {
                            // Сообщения о том надо ли принимать файл
                            DialogResult dialogResult = MessageBox.Show("Прниять файл " + packet.Data.FileName + 
                                "от клиента № " + packet.Sender + " размером " + packet.Data.FileLenght/1024/1024 + "МБ?",
                                "Передача файла", MessageBoxButtons.YesNo);

                            if (dialogResult == DialogResult.Yes)
                            {
                                // Выставляет флаг приема 
                                _isRecivingFile = true;

                                //Высылает разрешение на отправку файла  
                                AddPacketToQueue(new byte[] { 0x00 }, packet.Sender, 0x41);
                                
                                //Обнулить счетчик пакетов файла
                                _countOfFilePackets = 0;

                                _receivingFileName = packet.Data.FileName;
                                _receivingFileSize = packet.Data.FileLenght;

                                // Запускает поток ожидания пакетов файла
                                _waitForFilePacketThread = new Thread(WaitForFilePacket);
                                _waitForFilePacketThread.Start();

                                // Создает файл если его еще нет
                                CreateFile(_receivingFileName);
#if DEBUG
                                MessageBox.Show("Размер файла = " + _receivingFileSize.ToString() + '\n' +
                                                "Имя файла = " + _receivingFileName);
#endif
                            }
                            else
                            {
                                _isRecivingFile = false;
                                //Отказ отправки файла
                                SendFileTransferCancel(packet.Sender);  
                            }                            
                        }
                        else
                        {
#if DEBUG
                            MessageBox.Show("Клиенту № " + packet.Sender + " Было отказано в передачи файла "
                                + packet.Data.FileName + " размером " + packet.Data.FileLenght / 1024 / 1024
                                + "МБ, так как в данный момент уже осуществляется прием другого файла ");
#endif                                                                      
                        }
                    }
                        break;

                    // Обработка пакета файла
                    case "FileData" :
                        {
                            // Если совпал номер пакета и разрешено получение файлов и файл существует
                            if (_countOfFilePackets == packet.Data.PacketNumber && _isRecivingFile && File.Exists(_receivingFileFullName))
                            {
                                // Устанавливает что пакет получен
                                _waitForFilePacketEvent.Set();

                                // Выслать подверждение получения пакета
                               SendAcknowledge(packet);

                                // Разархивация данных
                                packet.Data.Content = Compressor.Unzip(packet.Data.Content);

                                // Запись данных в файл
                                ByteArrayToFile(_receivingFileFullName, packet.Data.Content);

                                // Инкрементирует число принятых пакетов
                                _countOfFilePackets++;

                                // Если пакет последний в цепочке
                                if (packet.Data.LastPacket == 0x4C)
                                {
#if DEBUG
                                    MessageBox.Show("Файл принят! Всего пакетов в файле = " + _countOfFilePackets);
#endif
                                    SendFileTransferCompleted(packet.Sender);
                                    _isRecivingFile = false;
                                }
                            }
                            else
                            {
#if DEBUG
                                MessageBox.Show("Пакет файла: не совпал номер пакета, принятый номер " + packet.ByteData[2] + "сохраненый номер " + _countOfFilePackets);
#endif
                            }
                        }
                        break;
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

        public void CancelRecivingFile()
        {
            if (_isRecivingFile)
            {
                // Установить что файл больше не принимается
                _isRecivingFile = false;
                // Запрещает передавать файла до запроса на отправку
                _allowSendingFile = false;
                // Удаляет недопринятый файл
                DeleteFile(_receivingFileFullName);
                // Обнуляет счетчик
                _countOfFilePackets = 0;
                // Высылает уведемление о прекращении передачи файла
                SendFileTransferCancel(_fileRecipient);
            }           
        }

        public void CancelSendingFile()
        {
            if (_isSendingFile)
            {
                // Устанавливает что файл не передается
                _isSendingFile = false;
                // Запрещает передавать файла до запроса на отправку
                _allowSendingFile = false;
                // Ждет завершения потока что бы он не слал больше пакетов. Надо ли оно тут?
                _fileSenderThread.Join(1000);
                // Отчищает очередь на отправку файла
                _outFilePacketsQueue.Clear();
                // Высылает уведемление о прекращении передачи файла
                SendFileTransferCancel(_fileRecipient);
                // Обнуляет счетчик
                _countOfFilePackets = 0;
            }            
        }

        private bool DeleteFile(string file)
        {
            int attempts = 0;

            while (attempts < 3)
            {
                if (File.Exists(file))
                {
                    try
                    {
                        lock (_receivingFileFullName)
                        {
                            File.Delete(file);
                        }
                        return true;
                    }
                    catch (Exception)
                    {  
                        throw;
                    }
                }
                Thread.Sleep(1);
                attempts++;
            }

            return false;
        }

        // TO DO: Вынести нижеследующие методы из файла и сделать статичными

        private bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    return false;
                }


                lock (_receivingFileFullName)
                {
                    // Открывает файл в режими для записи в конец файла
                    FileStream _FileStream = new FileStream(fileName, FileMode.Append, FileAccess.Write);

                    // Записывает блок байтов в поток и следовательно в файл
                    _FileStream.Write(byteArray, 0, byteArray.Length);

                    // Закрывает поток
                    _FileStream.Close();
                }
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
