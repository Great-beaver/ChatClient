﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using ChatClient;
using ChatClient.Main;
using ChatClient.Main.Packet;
using ChatClient.Main.Packet.DataTypes;

namespace Chat.Main
{
   public class ClientPort : IDisposable
    {
        private SerialPort _comPortReader;
        private SerialPort _comPortWriter;
        private Thread _readThread;
        private Thread _writeThread;
        private Thread _selfCheckingThread;
        private Thread _fileSenderThread;
        private Thread _waitForFilePacketThread;
        public byte ClietnId { get; private set; }  
        private int _outMessageQueueSize = 200;
        private int _filePacketSize = 1024;
        private Client[] _clientArray = new Client[5];

       // Событие при получении пакета файла
       private ManualResetEvent _waitForFilePacketEvent = new ManualResetEvent(false);
       // Таймаут ожидания пакета файла в мс
       private int _waitForFilePacketTimeout = 10000;
       
       // Таймер для ожидания ответа о передачи файла
       private Timer _waitFileTransferAnswerTimer;
       // Время таймаута для ответа получателя, в мс
       private int _waitFileTransferAnswerTimeOut = 30000;
       // Флаг устанавливается когда получен запрос на передачу файла и снимается когда пользователь посылает ответ
       private bool _waitFileTransferAnswer = false;
        
       // Отражает состояние принимается ли или передается ли файл
       public bool IsRecivingFile { get; private set; }
              
       // Определяет сколько времение в мс потоки будут находится в состоянии сна
       private int _sleepTime = 1;    

       // Данные о файле для передачи
       public FileInfo FileToTransfer { get; private set; }

       // Данные о файле для приема
       private string _receivingFileName = "";
       // Имя учитывая путь к файлу
       public string ReceivingFileFullName = "";
       public long ReceivingFileSize { get; private set; }  
       // Отправитель файла
       private byte _fileSender = 255;

       // Размер уже обработанного файла, полученного или отправленного
       public long ProcessedFileSize { get; private set; }

       public ClientPort(string readerPortName,string writerPortName, byte id, int portSpeed)
        { 
            _comPortReader = new SerialPort();
            _comPortReader.PortName = readerPortName;
            _comPortReader.BaudRate = portSpeed;
            _comPortReader.Parity = Parity.None;
            _comPortReader.DataBits = 8;
            _comPortReader.StopBits = StopBits.One;
            _comPortReader.Handshake = Handshake.None;
            _comPortReader.ReadTimeout = 500;
            _comPortReader.WriteTimeout = 500;
            _comPortReader.WriteBufferSize = 65000;
            _comPortReader.ReadBufferSize = 65000;
            _comPortReader.ReadTimeout = 5000;
            
           _comPortWriter = new SerialPort();
           _comPortWriter.PortName = writerPortName;
           _comPortWriter.BaudRate = portSpeed;
           _comPortWriter.Parity = Parity.None;
           _comPortWriter.DataBits = 8;
           _comPortWriter.StopBits = StopBits.One;
           _comPortWriter.Handshake = Handshake.None;
           _comPortWriter.ReadTimeout = 500;
           _comPortWriter.WriteTimeout = 500;
           _comPortWriter.WriteBufferSize = 65000;
           _comPortWriter.ReadBufferSize = 65000;

            // Особая уличная кодировка для правильной отправки байтов чьё значение больше 127-ми
            _comPortReader.Encoding = Encoding.GetEncoding(28591);
            _comPortWriter.Encoding = Encoding.GetEncoding(28591);

           ProcessedFileSize = 0;

           IsRecivingFile = false;

            ClietnId = id;

            for (int i = 0; i < 5; i++)
            {
                _clientArray[i] = new Client((byte)i,ClietnId, _outMessageQueueSize, _comPortWriter);

                // Подписывает на события от клиента
                _clientArray[i].AcknowledgeRecived +=
                    new EventHandler<MessageRecivedEventArgs>(ClientAcknowledgeRecived);
            }

            _waitFileTransferAnswerTimer = new Timer(WaitFileAnswerTransfer);

            ReOpenPort(_comPortReader);
            ReOpenPort(_comPortWriter);
           
            Client.Continue = true;
            _readThread = new Thread(Read);
            _readThread.Start();

            _selfCheckingThread = new Thread(SelfChecking);
            _selfCheckingThread.Start();           
        }

       private void WaitFileAnswerTransfer(object state)
        {
            // Если был отправлен запрос на передачу файла
            if (Client.AllowSendingFile)
            {
                // Таймаут ожидания ответа от получателя файла о приеме файла
                Client.AllowSendingFile = false;
                OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileTransferDenied, FileToTransfer.Name, Client.FileRecipient));  
            }

            // Если был получен запрос на передачу файла 
            if (_waitFileTransferAnswer)
            {
                _waitFileTransferAnswer = false;
                OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileTransferCanceledRecipientSide, _receivingFileName, _fileSender));  
            }

        }

       // Делегат обработчика  текстовых сообщений 
       public delegate void ReciveAcknowledgeDelegate(MessageType type, string text, byte sender);

       // Метод обработки текстовых сообщений 
       void ReciveAcknowledge(MessageType type, string text, byte sender)
        {
            // Событие передает тип пакета и текст для пользователя
            OnMessageRecived(new MessageRecivedEventArgs(type, text, sender));
        }

       // Получение данных и вызов обработчика для текстовых сообщений
       void ClientAcknowledgeRecived(object sender, MessageRecivedEventArgs e)
        {
            ReciveAcknowledge(e.MessageType, e.MessageText, e.Sender);
        }

       public void Dispose()
        {
            Client.Continue = false;

            if (_readThread != null)
            _readThread.Join(1000);

            if (_writeThread != null)
            _writeThread.Join(1000);

            if (_fileSenderThread != null)
            _fileSenderThread.Join(1000);

            if (_waitForFilePacketThread != null)
            _waitForFilePacketThread.Join(1000);
        }

       // Получение сообщения 

       public event EventHandler<MessageRecivedEventArgs> MessageRecived;

       private void OnMessageRecived(MessageRecivedEventArgs e)
        {
            EventHandler<MessageRecivedEventArgs> handler = MessageRecived;
            if (handler != null) handler(this, e);
        }

       // Получение запроса на передачу файла

       public event EventHandler<FileRequestRecivedEventArgs> FileRequestRecived;

       private void OnFileRequestRecived(FileRequestRecivedEventArgs e)
        {
            EventHandler<FileRequestRecivedEventArgs> handler = FileRequestRecived;
            if (handler != null) handler(this, e);
        }

       private void SelfChecking()
       {
           while (Client.Continue)
           {
               if (IsPortAvailable(_comPortReader))
                   OnMessageRecived(new MessageRecivedEventArgs(MessageType.ReadPortAvailable, _comPortReader.PortName, 255));
               else
               {
                   OnMessageRecived(new MessageRecivedEventArgs(MessageType.ReadPortUnavailable, _comPortReader.PortName, 255));
                   ReOpenPort(_comPortReader);
               }

               if (IsPortAvailable(_comPortWriter))
                   OnMessageRecived(new MessageRecivedEventArgs(MessageType.WritePortAvailable, _comPortWriter.PortName, 255));
               else
               {
                   OnMessageRecived(new MessageRecivedEventArgs(MessageType.WritePortUnavailable, _comPortWriter.PortName, 255));
                   ReOpenPort(_comPortWriter);
               }

               Thread.Sleep(2000);
           }
       }

       public static bool IsPortAvailable(SerialPort port)
       {
           bool portExist = false;

           foreach (string value in SerialPort.GetPortNames())
           {
               if (value == port.PortName) portExist = true;
           }

           if (portExist && port.IsOpen) return true;

           return false;
       }

       private void WaitForFilePacket()
        {
            // Пока стоит флаг приема файла
            while (IsRecivingFile && Client.Continue)
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
                    if (IsRecivingFile)
                    {
                        // Событие - отправитель файла не доступен    
                        OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileReceivingTimeOut, _receivingFileName, _fileSender));

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
            var stream = File.OpenRead(FileToTransfer.FullName);
            var buffer = new byte[_filePacketSize];

           ProcessedFileSize = 0;

            // Количество пакетов в файле
            long totalCountOfPackets = FileToTransfer.Length / buffer.Length;

            // Количество уже отправленных пакетов
            long countOfSendedPackets = 0;

            // TO DO: Возможно стоит удалить эту строку
            // Отчищает очередь пакетов файла на передачу 
                _clientArray[Client.FileRecipient].OutFilePacketsQueue =  new ConcurrentQueue<Packet>();
 
            Client.CountOfFilePackets = 0;

            while (true && Client.IsSendingFile && Client.Continue)
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
                    if (_clientArray[Client.FileRecipient].OutFilePacketsQueue.Count < _clientArray[Client.FileRecipient].QueueSize)
                    {
                       // ProcessedFileSize += _filePacketSize;
                        if (!(Client.IsSendingFile && Client.Continue))
                        {
                            return;
                        }
                        // Если эот не последний пакет то отправляем весь буфер
                        if (countOfSendedPackets < totalCountOfPackets)
                        {
                            SendFilePacket(buffer, (byte) toId,Client.CountOfFilePackets);
                            countOfSendedPackets++;
                            Client.CountOfFilePackets++;
                        }
                        else 
                        { 
                            // Иначе отправляет только столько байт сколько было считано и этот пакет считается последним 
                            byte[] lastPacket = new byte[size];
                            Array.Copy(buffer, 0, lastPacket, 0, lastPacket.Length);
                            SendFilePacket(lastPacket, (byte)toId, Client.CountOfFilePackets,true);

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

       public bool SendTextMessage(string message, byte toId)
        {
           // Если очередь не пуста не позвоялть добавлять в нее новые текстовые сообщения
            if (!_clientArray[(int)toId].OutMessagesQueue.IsEmpty) return false;

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

            _clientArray[(int)toId].AddPacketToQueue(messagePacket, ClietnId, option1, option2);

           return true;
        }

       public void SendFileTransferRequest(string filePath, byte toId)
        {
            // TO DO: Отправлять запрос только если не идет работа с файлом

            // Структура пакета запроса на передачу файла
            // | Тип пакета |   Длина файла   | Имя файла |
            // |   1 байт   |      8 байт     |  0 - 1024 |

            FileToTransfer = new FileInfo(filePath);

            Client.AllowSendingFile = true;

            // Устанавливает кому будет передаваться файл
            Client.FileRecipient = toId;

            if (Encoding.UTF8.GetBytes(FileToTransfer.Name).Length >= 1023)
            {
                // Событие - сообщение о ошибке 
                OnMessageRecived(new MessageRecivedEventArgs(MessageType.Error, "Слишком длинное имя файла", 0));
                return;
            }

            if (!(File.Exists(filePath)))
            {
                // Событие - сообщение о ошибке 
                OnMessageRecived(new MessageRecivedEventArgs(MessageType.Error, "Файла не существует", 0));
                return;
            }

            // Устанавливает длину конкретного пакета
            byte[] packet = new byte[9 + Encoding.UTF8.GetBytes(FileToTransfer.Name).Length];

            // Тип сообщения - запрос на передачу файла
            packet[0] = 0x52;

            // Вставляет длину файла в пакет
            Array.Copy(BitConverter.GetBytes(FileToTransfer.Length), 0, packet, 1, 8);

            //Вставляет имя файла в пакет
            Array.Copy(Encoding.UTF8.GetBytes(FileToTransfer.Name), 0, packet, 9, Encoding.UTF8.GetBytes(FileToTransfer.Name).Length);

            _clientArray[(int)toId].AddPacketToQueue(packet, ClietnId);

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

            _clientArray[(int)toId].AddPacketToQueue(Packet, ClietnId, option1, option2);
        }

       private void SendFileTransferCancel(byte toId)
        {
            _clientArray[toId].AddPacketToQueue(new byte[] { 0x00 }, ClietnId, 0x18);
        }

       private void SendFileTransferAllow(byte toId)
        {
            _clientArray[toId].AddPacketToQueue(new byte[] { 0x00 }, ClietnId, 0x41); 
        }

       public void AllowFileTransfer ()
        {
            if (!_waitFileTransferAnswer) return;

           ProcessedFileSize = 0;
            _waitFileTransferAnswer = false;
            // Событие - начат прием файла
            OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileReceivingStarted, _receivingFileName, _fileSender));
            // Выставляет флаг приема 
            IsRecivingFile = true;
            //Высылает разрешение на отправку файла  
            SendFileTransferAllow(_fileSender);
            //Обнулить счетчик пакетов файла
            Client.CountOfFilePackets = 0;
            // Запускает поток ожидания пакетов файла
            _waitForFilePacketThread = new Thread(WaitForFilePacket);
            _waitForFilePacketThread.Start();
            // Создает файл если его еще нет
            CreateFile(_receivingFileName);
        }
       
       public void DenyFileTransfer ()
        {
            if (!_waitFileTransferAnswer) return;

            _waitFileTransferAnswer = false;
            // Событие - прием файла отклонен
            OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileTransferCanceledRecipientSide, _receivingFileName, _fileSender));
            IsRecivingFile = false;
            //Отказ отправки файла
            SendFileTransferCancel(_fileSender);  
        }

       private void SendAcknowledge(Packet packet)
        {
            _clientArray[(int)packet.Header.Sender].AddPacketToQueue(BitConverter.GetBytes(Crc16.ComputeChecksum(packet.ByteData)), ClietnId, 0x06, 0x00, true);
        }

       private void SendFileTransferCompleted(byte toId)
        {
          _clientArray[(int)toId].AddPacketToQueue(new byte[] { 0x00 }, ClietnId, 0x04);
        }

       private void Read()
        {
            while (Client.Continue)
            {
                try
                {
                    _comPortReader.ReadTo("\xAA\x55");

                    // Считывание данных для создания пакета
                    // Здесь важен строгий порядок считывания байтов, точно как в пакете.
                    byte recipient = (byte)_comPortReader.ReadByte();
                    byte sender = (byte)_comPortReader.ReadByte();
                    ushort dataLenght = BitConverter.ToUInt16(
                        new byte[] { (byte)_comPortReader.ReadByte(), (byte)_comPortReader.ReadByte() }, 0);
                    byte option1 = (byte)_comPortReader.ReadByte();
                    byte option2 = (byte)_comPortReader.ReadByte();
                    ushort crc = BitConverter.ToUInt16(
                        new byte[] { (byte)_comPortReader.ReadByte(), (byte)_comPortReader.ReadByte() }, 0);

                  // Счетчик количества итерация цикла while 
                  int count = 0;
                  while (_comPortReader.BytesToRead < dataLenght)
                  {
                      count++;
                      Thread.Sleep(_sleepTime);
                      if (count > dataLenght)
                      {
                          break;
                      }
                  }

                    byte[] data = new byte[dataLenght];
                    _comPortReader.Read(data, 0, dataLenght);

                    Packet packet = new Packet(new Header(recipient, sender, option1, option2), data);

                    // Проверка crc и id клиента, то есть предназначен ли этот пакет этому клиенту.
                    if (packet.Header.Crc == crc && packet.Header.Recipient == ClietnId)
                    {
                        //Функция разбора пакета
                        ParsePacket(packet);
                    }
                    else
                    {
                        // MessageBox.Show("Hash or ID NOT matches!");
                        // MessageBox.Show(packet.PacketInfo());
                    }
                    
                }

                catch (InvalidOperationException)
                {
                    IsPortAvailable(_comPortReader);
                    // Передает событие с текстом ошибки
                  //  OnMessageRecived(new MessageRecivedEventArgs(MessageType.ReadPortUnavailable,  _comPortReader.PortName, 255));
                  //  ReOpenPort(_comPortReader);
                    Thread.Sleep(3000);
                }

                catch (Exception)
                {

                }

            }
        }

       private void ParsePacket(Packet packet)
        {
            // Функция вовращает true если найдена опция и дальнейший разбор данных не требуется.
                if (ParseOptions(packet))
                {
                    return;
                }

            switch (packet.Data.Type)
            {
                // Обработка пакета текстового сообщения 
                case  DataType.Text: 
                    {
                        // Определяет необходимость разархивирования
                        if (packet.Header.Option2 == PacketOption2.Compressed)
                        {
                            packet.Data.Content = Compressor.Unzip(packet.Data.Content);
                        }

                        // Событие передает тип пакета и текст для пользователя
                        OnMessageRecived(new MessageRecivedEventArgs(MessageType.Text, Encoding.UTF8.GetString(packet.Data.Content), packet.Header.Sender));

                        // Выслать ACK
                        SendAcknowledge(packet);
                    }
                    break;

                // Обработка пакета запроса на передачу файла
                case DataType.FileRequest:
                    {
                        // Выслать подверждение получения пакета
                       SendAcknowledge(packet);

                        // Если не принимается и не передается файл
                        if (!Client.IsSendingFile && !IsRecivingFile)
                        {
                            var ea = new FileRequestRecivedEventArgs(packet.Data.FileName, packet.Data.FileLenght, packet.Header.Sender);

                            OnFileRequestRecived(ea);

                            _waitFileTransferAnswer = true;

                            // Сохраняет параметры файла
                            _receivingFileName = packet.Data.FileName;
                            ReceivingFileSize = packet.Data.FileLenght;
                            // Сохраняет id отправителя файла
                            _fileSender = packet.Header.Sender;

                            _waitFileTransferAnswerTimer.Change(_waitFileTransferAnswerTimeOut, Timeout.Infinite);

                        }
                        else
                        {
#if DEBUG
                            SendFileTransferCancel(packet.Header.Sender);

                            // Событие - сообщение о ошибке 
                            OnMessageRecived(new MessageRecivedEventArgs(MessageType.Error, "Клиенту № " + packet.Header.Sender + " Было отказано в передачи файла "
                                + packet.Data.FileName + " размером " + packet.Data.FileLenght / 1024 / 1024
                                + "МБ, так как в данный момент уже осуществляется прием другого файла ", 0));
#endif                                                                      
                        }
                    }
                        break;

                    // Обработка пакета файла
                    case DataType.FileData :
                        {
                            // Если совпал номер пакета и разрешено получение файлов и файл существует и id отправителя совпал с id отправителя файла
                            if (Client.CountOfFilePackets == packet.Data.PacketNumber && IsRecivingFile && File.Exists(ReceivingFileFullName) && packet.Header.Sender == _fileSender)
                            {
                                ProcessedFileSize += _filePacketSize;
                                // Устанавливает что пакет получен
                                _waitForFilePacketEvent.Set();
                                // Выслать подверждение получения пакета
                                SendAcknowledge(packet);
                                // Разархивация данных
                                packet.Data.Content = Compressor.Unzip(packet.Data.Content);
                                // Запись данных в файл
                                ByteArrayToFile(ReceivingFileFullName, packet.Data.Content);
                                // Инкрементирует число принятых пакетов
                                Client.CountOfFilePackets++;
                                // Если пакет последний в цепочке
                                if (packet.Data.LastPacket == 0x4C)
                                {
                                    // Событие о заверщении приема файла 
                                    OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileReceivingComplete, _receivingFileName, packet.Header.Sender));
                                    SendFileTransferCompleted(packet.Header.Sender);
                                    IsRecivingFile = false;
                                }
                            }
                            else
                            {
#if DEBUG
                                // Событие - сообщение о ошибке 
                                OnMessageRecived(new MessageRecivedEventArgs(MessageType.Error, "Пакет файла: не совпал номер пакета, принятый номер " + packet.ByteData[2] + 
                                    "сохраненый номер " + Client.CountOfFilePackets, 0));
#endif
                            }
                        }
                        break;
            }
        }

       private bool ParseOptions (Packet packet)
        {
           switch (packet.Header.Option1)
           {
               case PacketOption1.Acknowledge:
                   {
                       // Обработка пакета подверждения доставки сообщения
                       // Если первый бит опций равен ACK и CRC в пакете совпала с последним отправленым пакетом для этого клиента
                       if (_clientArray[packet.Header.Sender].LastMessageCrc == BitConverter.ToUInt16(packet.ByteData, 0))
                       {
                           // Елси был доставлен пакет "запрос на передачу файла" то запустить таймер ожидания ответа
                           if (_clientArray[packet.Header.Sender].LastPacket.Data.Type == DataType.FileRequest)
                           {
                               OnMessageRecived(new MessageRecivedEventArgs(MessageType.WaitFileRecipientAnswer, FileToTransfer.Name, packet.Header.Sender));
                               _waitFileTransferAnswerTimer.Change(_waitFileTransferAnswerTimeOut, Timeout.Infinite);
                           }

                           if (_clientArray[packet.Header.Sender].LastPacket.Data.Type == DataType.FileData)
                           {
                               ProcessedFileSize += _filePacketSize;
                           }
                          
                           // Установить что последнее сообщение было доставлено
                           _clientArray[packet.Header.Sender].AnswerEvent.Set();
                           return true;
                       }
                   }
                   break;

               case PacketOption1.FileTransferAllowed:
                   {
                       // Обработка пакета разрешения на передачу файла
                       // Если получено разрешение на передачу файлов и отправитель пакета является тем кому был отправлен запрос на передачу
                       if (Client.AllowSendingFile && Client.FileRecipient == packet.Header.Sender)
                       {
                           // Событие о разрещении на передачу файла
                           OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileTransferAllowed, FileToTransfer.Name, packet.Header.Sender));

                           // Начать отправку файла
                           // Отчищает очередь пакетов файла на передачу 
                           // TO DO: Добавить отчистку очереди?

                           // Запрещает отправлять файла на последующие запросы до завершения передачи
                           Client.AllowSendingFile = false;
                           // Устанавливает что идет передача файла
                           Client.IsSendingFile = true;
                           // Обнуляет счетчик пакетов
                           Client.CountOfFilePackets = 0;
                           // Запускает поток для упаковки файла в пакеты и добавления их в очередь
                           _fileSenderThread = new Thread(FileSender);
                           _fileSenderThread.Start(packet.Header.Sender);
                       }
                       // Выслать подверждение получения пакета
                       SendAcknowledge(packet);
                       return true;
                   }
                   break;

               case PacketOption1.FileTransferDenied:
                   {
                       // Если получен пакет отмены передачи файла
                       // Если файл принимался
                       if (IsRecivingFile)
                       {
                           // Событие отмены перадачи файла отправителем
                           OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileTransferCanceledBySender, _receivingFileName, packet.Header.Sender));
                           
                           CancelRecivingFile();
                           // Выслать подверждение получения пакета
                           SendAcknowledge(packet);
                           return true;
                       }
                       // Если файл отправлялся
                       if (Client.IsSendingFile)
                       {
                           // Событие отмены перадачи файла получателем
                           OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileTransferCanceledByRecipient, FileToTransfer.Name, packet.Header.Sender));
                           CancelSendingFile();
                           // Выслать подверждение получения пакета
                           SendAcknowledge(packet);
                           return true;
                       }
                       // Если был отправлен только запрос на передачу файла 
                       if (Client.AllowSendingFile)
                       {
                           Client.AllowSendingFile = false;
                           // Событие отказа от приема файла 
                           OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileTransferDenied, FileToTransfer.Name, packet.Header.Sender));
                       }

                       if (_waitFileTransferAnswer)
                       {
                           _waitFileTransferAnswer = false;
                           // Отправитель отменил запрос
                           OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileRequestCanceledRecipientSide, _receivingFileName, packet.Header.Sender));
                       }

                       // Выслать подверждение получения пакета
                       SendAcknowledge(packet);
                       return true;
                   }
                   break;

               case PacketOption1.FileTransferCompleted :
                   {
                       if (packet.Header.Option1 == PacketOption1.FileTransferCompleted)
                       {
                           ProcessedFileSize = 0;
                           Client.IsSendingFile = false;
                           // Событие доставки файла
                           OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileSendingComplete, FileToTransfer.Name, packet.Header.Sender));
                           SendAcknowledge(packet);
                           return true;
                       }
                   }
                   break;
           }
           return false;
        }

       public bool CreateFile (string fileName) 
        {
            // Создает папку в "Мои документы" если ее нет
            string md = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\ChatRecivedFiles\\";
            if (Directory.Exists(md ) == false)
            {
                Directory.CreateDirectory(md);
            }

            // TO DO: Вернуть код диалога о перезаписи файла
            // Проверяет наличие файла
       //     if ((File.Exists(md+fileName)))
       //     {
       //         // Если файл существует то предлагает его перезаписать
       //         DialogResult dialogResult = MessageBox.Show("Файл " + fileName  + " уже существует, перезаписать его?", "Перезаписать файл?", MessageBoxButtons.YesNo);
       //
       //         // В случае отказа прекращает фанкцию
       //         if (dialogResult == DialogResult.No)
       //         {
       //             // TO DO: Добавить код отмены приема файла
       //             return false;
       //         }               
       //     }

            try
            {
                FileStream fs = File.Create(md + fileName);
                ReceivingFileFullName = md + fileName;
                fs.Close();
            }
            catch (Exception _Exception)
            {
                return false;
            }

            return false;
        }

       public void CancelRecivingFile()
        {
            if (IsRecivingFile)
            {
                ProcessedFileSize = 0;
                // Установить что файл больше не принимается
                IsRecivingFile = false;
                // Запрещает передавать файла до запроса на отправку
                Client.AllowSendingFile = false;
                // Удаляет недопринятый файл
                DeleteFile(ReceivingFileFullName);
                // Обнуляет счетчик
                Client.CountOfFilePackets = 0;
                // Высылает уведемление о прекращении передачи файла
                SendFileTransferCancel(_fileSender);
                // Событие - прием файла отклонен
                OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileTransferCanceledRecipientSide, _receivingFileName, _fileSender));
            }           
        }

       public void CancelSendingFile()
        {
            if (Client.IsSendingFile)
            {
                ProcessedFileSize = 0;
                // Устанавливает что файл не передается
                Client.IsSendingFile = false;
                // Запрещает передавать файла до запроса на отправку
                Client.AllowSendingFile = false;
                // Отчищает очередь на отправку файла
                _clientArray[Client.FileRecipient].OutFilePacketsQueue = new ConcurrentQueue<Packet>();
                // Высылает уведемление о прекращении передачи файла
                SendFileTransferCancel(Client.FileRecipient);
                // Обнуляет счетчик
                Client.CountOfFilePackets = 0;
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Событие - передача файла отменена
                OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileTransferCanceledSenderSide, FileToTransfer.Name, Client.FileRecipient));
            }            
        }

       public void CancleFileRequest()
       {
           if (Client.AllowSendingFile)
           {
               // Запрещает передавать файла до запроса на отправку
               Client.AllowSendingFile = false;

               // Высылает уведемление о прекращении передачи файла
               SendFileTransferCancel(Client.FileRecipient);

               // Событие - отмена запроса
               OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileRequestCanceledSenderSide, FileToTransfer.Name, Client.FileRecipient));

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
                        lock (ReceivingFileFullName)
                        {
                            File.Delete(file);
                        }
                        return true;
                    }
                    catch (Exception)
                    {  
                        // TO DO: do something
                    }
                }
                Thread.Sleep(1);
                attempts++;
            }

            return false;
        }

       private bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    return false;
                }


                lock (ReceivingFileFullName)
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

       public static bool ReOpenPort(SerialPort port)
        {
            try
            {
                port.Close();
                port.Open();

                return port.IsOpen;
            }
            catch (Exception)
            {
                return false;
            }   
        }

    }
}
