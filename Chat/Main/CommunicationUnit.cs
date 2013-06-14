using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using Chat.Helpers;
using Chat.Main.Packet;
using Chat.Main.Packet.DataStructs;
using Chat.Main.Packet.DataTypes;


namespace Chat.Main
{
   public class CommunicationUnit : IDisposable
    {
       private SerialPort[] _readPorts;
       private SerialPort _comPortWriter;
       private Thread _readThread;
       private Thread _writeThread;
       private Thread _routerThread;
       private ConcurrentQueue<Packet.Packet> _routerQueue;
       private Thread _selfCheckingThread;
       private Thread _fileSenderThread;
       private Thread _waitForFilePacketThread;
       public byte ClietnId { get; private set; }  
       private int _outMessageQueueSize = 200;
       private int _filePacketSize = 1024;
       private Dictionary<int, Client> _clientArray = new Dictionary<int, Client>();
       //private Client[] _clientArray = new Client[5];
       private BroadcastManager _broadcastManager;
       private FileStream _fileStream;
       
       private bool _isServer = false;

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

       /// <summary>
       /// Конструктор клиентской части
       /// </summary>
       /// <param name="readerPortName">Имя считываюшего порта</param>
       /// <param name="writerPortName">Имя порта для записи</param>
       /// <param name="id">Идентификатор клиента</param>
       /// <param name="portSpeed">Скорость портов</param>
       public CommunicationUnit(string readerPortName,string writerPortName, byte id, int portSpeed)
        {
            _readPorts = new SerialPort[1];

            _readPorts[0] = new SerialPort();
            _readPorts[0].PortName = readerPortName;
            _readPorts[0].BaudRate = portSpeed;
            _readPorts[0].Parity = Parity.None;
            _readPorts[0].DataBits = 8;
            _readPorts[0].StopBits = StopBits.One;
            _readPorts[0].Handshake = Handshake.None;
            _readPorts[0].ReadTimeout = 500;
            _readPorts[0].WriteTimeout = 500;
            _readPorts[0].WriteBufferSize = 65000;
            _readPorts[0].ReadBufferSize = 65000;
            _readPorts[0].ReadTimeout = 5000;
            
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
            _readPorts[0].Encoding = Encoding.GetEncoding(28591);
            _comPortWriter.Encoding = Encoding.GetEncoding(28591);

           ProcessedFileSize = 0;

           IsRecivingFile = false;

            ClietnId = id;

          
           // TODO: Создавать клиентов по количеству доступных клиентов
          _broadcastManager = new BroadcastManager(4);

           // Создание объекто для каждого клиента, обхект с номер равным номеру текущего клиента используется для широковещательных сообщений 
            for (int i = 0; i < 5; i++)
            {
                //  _clientArray[i] = new Client((byte)i,ClietnId, _outMessageQueueSize, _comPortWriter);
                _clientArray.Add(i, new Client((byte)i, ClietnId, _outMessageQueueSize, _comPortWriter));

                // Подписывает на обработку события получения состояния  доставки пакета
                _clientArray[i].AcknowledgeRecived +=
                    new EventHandler<MessageRecivedEventArgs>(ClientAcknowledgeRecived);

                _clientArray[i].AcknowledgeRecived +=
                    new EventHandler<MessageRecivedEventArgs>(_broadcastManager.ReceiveAcknowledge);
            }

            _waitFileTransferAnswerTimer = new Timer(WaitFileAnswerTransfer);

            ReOpenPort(_readPorts[0]);
            ReOpenPort(_comPortWriter);
           
            Client.Continue = true;
            _readThread = new Thread(Read);
            _readThread.Start(_readPorts[0]);

            _selfCheckingThread = new Thread(SelfChecking);
            _selfCheckingThread.Start();           
        }

       // TODO Убрать возможность определять ID для сервера сделать его всегда 0
       /// <summary>
       /// Конструктор серверной части 
       /// </summary>
       /// <param name="readerPortName1">Имя первого считываюшего порта </param>
       /// <param name="readerPortName2">Имя второго считываюшего порта</param>
       /// <param name="readerPortName3">Имя третьего считываюшего порта</param>
       /// <param name="readerPortName4">Имя четвертого считываюшего порта</param>
       /// <param name="writerPortName">Имя порта для записи</param>
       /// <param name="id">Идентификатор сервера, необходимо установить 0</param>
       /// <param name="portSpeed">Скорость портов</param>
       /// <param name="enabledClientIDs">ID достуаных клиентов</param>
       public CommunicationUnit(string readerPortName1, string readerPortName2, string readerPortName3, string readerPortName4, string writerPortName, byte id, int portSpeed, int[] enabledClientIDs)
       {
           _isServer = true;

           _readPorts = new SerialPort[4];

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
           
           _comPortWriter.Encoding = Encoding.GetEncoding(28591);
           
           ReOpenPort(_comPortWriter);

           _readPorts[0] = new SerialPort();
           _readPorts[0].PortName = readerPortName1;
         

           _readPorts[1] = new SerialPort();
           _readPorts[1].PortName = readerPortName2;

           _readPorts[2] = new SerialPort();
           _readPorts[2].PortName = readerPortName3;

           _readPorts[3] = new SerialPort();
           _readPorts[3].PortName = readerPortName4;

           Client.Continue = true;

           for (int i = 0; i < _readPorts.Length; i++)
           {
               _readPorts[i].BaudRate = portSpeed;
               _readPorts[i].Parity = Parity.None;
               _readPorts[i].DataBits = 8;
               _readPorts[i].StopBits = StopBits.One;
               _readPorts[i].Handshake = Handshake.None;
               _readPorts[i].ReadTimeout = 500;
               _readPorts[i].WriteTimeout = 500;
               _readPorts[i].WriteBufferSize = 65000;
               _readPorts[i].ReadBufferSize = 65000;
               _readPorts[i].ReadTimeout = 5000;

               _readPorts[i].Encoding = Encoding.GetEncoding(28591);

               ReOpenPort(_readPorts[i]);  
       
              _readThread = new Thread(Read);
              _readThread.Start(_readPorts[i]);
           }

           ProcessedFileSize = 0;

           IsRecivingFile = false;

           ClietnId = id;
          
           // Создание объекто для каждого клиента, обхект с номер равным номеру текущего клиента используется для широковещательных сообщений
       //  for (int i = 0; i < 5; i++)
       //  {
       //     // _clientArray[i] = new Client((byte)i, ClietnId, _outMessageQueueSize, _comPortWriter);
       //      _clientArray.Add(i, new Client((byte)i, ClietnId, _outMessageQueueSize, _comPortWriter));
       //
       //      // Подписывает на события от клиента
       //      _clientArray[i].AcknowledgeRecived +=
       //          new EventHandler<MessageRecivedEventArgs>(ClientAcknowledgeRecived);
       //
       //      _clientArray[i].AcknowledgeRecived +=
       //           new EventHandler<MessageRecivedEventArgs>(_broadcastManager.ReceiveAcknowledge);
       //
       //  }

           _broadcastManager = new BroadcastManager(enabledClientIDs.Length);

           foreach (int client in enabledClientIDs)
           {
               _clientArray.Add(client, new Client((byte)client, ClietnId, _outMessageQueueSize, _comPortWriter));

               // Подписывает на события от клиента
               _clientArray[client].AcknowledgeRecived +=
                   new EventHandler<MessageRecivedEventArgs>(ClientAcknowledgeRecived);

               _clientArray[client].AcknowledgeRecived +=
                    new EventHandler<MessageRecivedEventArgs>(_broadcastManager.ReceiveAcknowledge);
           }


           _waitFileTransferAnswerTimer = new Timer(WaitFileAnswerTransfer);

           _routerQueue = new ConcurrentQueue<Packet.Packet>();

           _routerThread = new Thread(Route);
           _routerThread.Start();

           _selfCheckingThread = new Thread(SelfChecking);
           _selfCheckingThread.Start();   

       }

       private void Route()
       {
           Packet.Packet outPacket;
           while (Client.Continue)
           {
               Thread.Sleep(_sleepTime);

               while (_routerQueue.TryDequeue(out outPacket))
               {
                   Client.TryWrite(_comPortWriter, outPacket);
               }
           }
       }

       private void WaitFileAnswerTransfer(object state)
        {
            // Если был отправлен запрос на передачу файла
            if (Client.AllowSendingFile)
            {
                // Таймаут ожидания ответа от получателя файла о приеме файла
                Client.AllowSendingFile = false;
                OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileTransferDenied, FileToTransfer.Name, Client.FileRecipient,0));  
            }

            // Если был получен запрос на передачу файла 
            if (_waitFileTransferAnswer)
            {
                _waitFileTransferAnswer = false;
                OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileTransferCanceledRecipientSide, _receivingFileName, _fileSender,0));  
            }

        }

       // Делегат обработчика  текстовых сообщений 
       public delegate void ReciveAcknowledgeDelegate(MessageType type, string text, byte sender);

       // Метод обработки текстовых сообщений 
       void ReciveAcknowledge(MessageType type, string text, byte sender, byte recipient)
        {
            // Событие передает тип пакета и текст для пользователя
            OnMessageRecived(new MessageRecivedEventArgs(type, text, sender, recipient));
        }

       // Получение данных и вызов обработчика для текстовых сообщений
       void ClientAcknowledgeRecived(object sender, MessageRecivedEventArgs e)
        {
            ReciveAcknowledge(e.MessageType, e.MessageText, e.Sender, e.Recipient);
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
               for (int i = 0; i < _readPorts.Length; i++)
               {
                   if (IsPortAvailable(_readPorts[i]))
                       OnMessageRecived(new MessageRecivedEventArgs(MessageType.ReadPortAvailable, _readPorts[i].PortName, (byte)i,0));
                   else
                   {
                       OnMessageRecived(new MessageRecivedEventArgs(MessageType.ReadPortUnavailable, _readPorts[i].PortName, (byte)i,0));
                       ReOpenPort(_readPorts[i]);
                   }  
               }

               

               if (IsPortAvailable(_comPortWriter))
                   OnMessageRecived(new MessageRecivedEventArgs(MessageType.WritePortAvailable, _comPortWriter.PortName, 255,0));
               else
               {
                   OnMessageRecived(new MessageRecivedEventArgs(MessageType.WritePortUnavailable, _comPortWriter.PortName, 255,0));
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

           if (portExist && port.IsOpen)
           {
               return true;
           }

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
                        OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileReceivingTimeOut, _receivingFileName, _fileSender,0));

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
            _clientArray[Client.FileRecipient].OutFilePacketsQueue = new ConcurrentQueue<Packet.Packet>();
 
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
           // Если очередь не пуста то не позвоялть добавлять в нее новые текстовые сообщения
            if (!_clientArray[(int)toId].OutMessagesQueue.IsEmpty) return false;

           if (message.Length > 1000)
           {
               // Событие - сообщение о ошибке 
               OnMessageRecived(new MessageRecivedEventArgs(MessageType.Error, "Слишком длинное сообщение", toId, 0));
               return false;
           }
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

           // // Массив байтов для отправки 
           // byte[] messagePacket = new byte[messageBody.Length+1];
           //
           // // Задает тип пакета, 0x54 - текстовое сообщение
           // messagePacket[0] = 0x54;
           //
           // // Копирует тело сообщения в позицию после Header'а, то есть в  messagePacket[1+]
           // Array.Copy(messageBody, 0, messagePacket, 1, messageBody.Length);


           Text text = new Text(messageBody);

           _clientArray[(int)toId].AddPacketToQueue(text, ClietnId, option1, option2);

           return true;
        }

       public bool SendBroadcastTextMessage(string message)
       {
           // Если очередь не пуста не позвоялть добавлять в нее новые текстовые сообщения
           //if (!_clientArray[(int)ClietnId].OutMessagesQueue.IsEmpty) return false;

           // Если в данный моммент уже отправляется сообщение то не отправлять нового
           if (!_broadcastManager.SetBusy())
           {
               return false;
           }

           if (message.Length > 1000)
           {
               // Событие - сообщение о ошибке 
               OnMessageRecived(new MessageRecivedEventArgs(MessageType.Error, "Слишком длинное сообщение", ClietnId, 0));
               return false;
           }

           // Структура пакета данных текстового сообщения 
           // | Тип пакета |   Данные   |
           // |   1 байт   | 0 - x байт | 

           byte option1 = 0x00;
           byte option2 = 0x00;

           // Переводит строку в массив байтов
           byte[] messageBody = Encoding.UTF8.GetBytes(message);

           // Архивирет сообщение если его длина более 1000 байт
           if (messageBody.Length > 1000)
           {
               messageBody = Compressor.Zip(messageBody); // Архивация
               option2 = 0x43; // Выставляем байт опций означающий что сообщение заархивировано
           }

          // // Массив байтов для отправки 
          // byte[] messagePacket = new byte[messageBody.Length + 1];
          //
          // // Задает тип пакета, 0x42 - широковешательное сообщение
          // messagePacket[0] = 0x42;
          //
          // // Копирует тело сообщения в позицию после Header'а, то есть в  messagePacket[1+]
          // Array.Copy(messageBody, 0, messagePacket, 1, messageBody.Length);
          //

           BroadcastText broadcastText = new BroadcastText(messageBody);

           // Отправка соообщения каждому клиенту
           foreach (var client in _clientArray)
           {
               if (client.Value.Id != client.Value.OwnerId)
               {
                   client.Value.AddPacketToQueue(broadcastText, ClietnId, option1, option2);   
               }
           }

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

            if (FileToTransfer.Length > 10485760)
           {
               // Событие - сообщение о ошибке 
               OnMessageRecived(new MessageRecivedEventArgs(MessageType.Error, "Размер файла не должен привышать 10 мегабайт.", toId, 0));
               return;
           }

            if (Encoding.UTF8.GetBytes(FileToTransfer.Name).Length >= 1023)
            {
                // Событие - сообщение о ошибке 
                OnMessageRecived(new MessageRecivedEventArgs(MessageType.Error, "Слишком длинное имя файла", toId, 0));
                return;
            }

            if (!(File.Exists(filePath)))
            {
                // Событие - сообщение о ошибке 
                OnMessageRecived(new MessageRecivedEventArgs(MessageType.Error, "Файла не существует", toId, 0));
                return;
            }

          //  // Устанавливает длину конкретного пакета
          //  byte[] packet = new byte[9 + Encoding.UTF8.GetBytes(FileToTransfer.Name).Length];
          //
          //  // Тип сообщения - запрос на передачу файла
          //  packet[0] = 0x52;
          //
          //  // Вставляет длину файла в пакет
          //  Array.Copy(BitConverter.GetBytes(FileToTransfer.Length), 0, packet, 1, 8);
          //
          //  //Вставляет имя файла в пакет
          //  Array.Copy(Encoding.UTF8.GetBytes(FileToTransfer.Name), 0, packet, 9, Encoding.UTF8.GetBytes(FileToTransfer.Name).Length);

            FileRequest fileRequest = new FileRequest(FileToTransfer.Length, FileToTransfer.Name);

            _clientArray[(int)toId].AddPacketToQueue(fileRequest, ClietnId);

        }

       private void SendFilePacket(byte[] packet, byte toId, byte packetNumber, bool lastPacketInChain = false)
        {
            // Структура пакета файла
            // | Тип пакета | Последний пакет | Номер пакета |  Данные  |
            // |   1 байт   |      1 байт     |    1 байт    |  0 - ... |

            byte option1 = 0x00;
            byte option2 = 0x00;

        //    byte[] messageBody = Compressor.Zip(packet); // Архивация
           byte[] messageBody = packet;


            
            // Массив байтов для отправки 
        //    byte[] Packet = new byte[messageBody.Length + 3];
            
            // Задает тип пакета, 0x46 - файл
          //  Packet[0] = 0x46;

          // // Указавает последний ли это пакет в последовательности 
          // if (lastPacketInChain)
          // {
          //     Packet[1] = 0x4C;
          // }
          // else
          // {
          //     Packet[1] = 0x00;    
          // }
          //
          // Packet[2] = packetNUmber;
          //
          // // Копирует тело сообщения в позицию после Header'а, то есть в  messagePacket[4+]
          // Array.Copy(messageBody, 0, Packet, 3, messageBody.Length);

           FileData fileData = new FileData(messageBody,lastPacketInChain,packetNumber);

           _clientArray[(int)toId].AddPacketToQueue(fileData, ClietnId, option1, option2);
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
            // Создает файл если его еще нет
            if (!CreateFile(ref _receivingFileName))
           {
               OnMessageRecived(new MessageRecivedEventArgs(MessageType.Error, "Не удалось создать файл", _fileSender, 0));
               CancelRecivingFile();
               return;
           }
           ProcessedFileSize = 0;
            _waitFileTransferAnswer = false;
            
            
            // Событие - начат прием файла
            OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileReceivingStarted, _receivingFileName, _fileSender,0));
            // Выставляет флаг приема 
            IsRecivingFile = true;
            //Высылает разрешение на отправку файла  
            SendFileTransferAllow(_fileSender);
            //Обнулить счетчик пакетов файла
            Client.CountOfFilePackets = 0;
            // Запускает поток ожидания пакетов файла
            _waitForFilePacketThread = new Thread(WaitForFilePacket);
            _waitForFilePacketThread.Start();
           try
           {
               // Открывает файл в режими для записи в конец файла
               _fileStream = new FileStream(ReceivingFileFullName, FileMode.Append, FileAccess.Write);
           }
           catch (Exception exception)
           {
               OnMessageRecived(new MessageRecivedEventArgs(MessageType.Error, exception.ToString(), _fileSender, 0));
           }

        }
       
       public void DenyFileTransfer ()
        {
            if (!_waitFileTransferAnswer) return;

            _waitFileTransferAnswer = false;
            // Событие - прием файла отклонен
            OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileTransferCanceledRecipientSide, _receivingFileName, _fileSender,0));
            IsRecivingFile = false;
            //Отказ отправки файла
            SendFileTransferCancel(_fileSender);  
        }

       private void SendAcknowledge(Packet.Packet packet)
        {
          //  _clientArray[(int)packet.Header.Sender].AddPacketToQueue(BitConverter.GetBytes(Crc16.ComputeChecksum(packet.ByteData)), ClietnId, 0x06, 0x00, true);

            Acknowledge acknowledge = new Acknowledge(Crc16.ComputeChecksum(packet.ByteData));

            _clientArray[(int)packet.Header.Sender].AddPacketToQueue(acknowledge, ClietnId, 0x06, 0x00, true);


        }

       private void SendFileTransferCompleted(byte toId)
        {
          _clientArray[(int)toId].AddPacketToQueue(new byte[] { 0x00 }, ClietnId, 0x04);
        }

       private void Read(object readport)
        {
            SerialPort readPort = (SerialPort)readport;

            while (Client.Continue)
            {
                try
                {
                    readPort.ReadTo("\xAA\x55");

                    // Считывание данных для создания пакета
                    // Здесь важен строгий порядок считывания байтов, точно как в пакете.
                    byte recipient = (byte)readPort.ReadByte();
                    byte sender = (byte)readPort.ReadByte();
                    ushort dataLenght = BitConverter.ToUInt16(
                        new byte[] { (byte)readPort.ReadByte(), (byte)readPort.ReadByte() }, 0);
                    byte option1 = (byte)readPort.ReadByte();
                    byte option2 = (byte)readPort.ReadByte();
                    ushort crc = BitConverter.ToUInt16(
                        new byte[] { (byte)readPort.ReadByte(), (byte)readPort.ReadByte() }, 0);

                  // Счетчик количества итерация цикла while 
                  int count = 0;
                  while (readPort.BytesToRead < dataLenght)
                  {
                      count++;
                      Thread.Sleep(_sleepTime);
                      if (count > dataLenght)
                      {
                          break;
                      }
                  }

                    byte[] data = new byte[dataLenght];
                    readPort.Read(data, 0, dataLenght);

                    var packet = new Packet.Packet(new Header(recipient, sender, option1, option2), data);

                     

                    if (_isServer)
                    {
                        if (packet.Header.Crc == crc)
                        {
                            if (packet.Header.Recipient != ClietnId || packet.Data.Type == DataType.BroadcastText)
                            {
                                
                              //  _clientArray[0].SendPacketNow(packet);

                                _routerQueue.Enqueue(packet);

                                // if ((packet.Data.Type == DataType.Text || packet.Data.Type == DataType.BroadcastText) && packet.Header.Option1 != PacketOption1.Acknowledge)  // До обновления бродкастинга

                                // Если пакет текстовый или пакет широковешательный и предназначен для этого клиента и это не ACK то пакет парсится
                                if ((packet.Data.Type == DataType.Text || (packet.Data.Type == DataType.BroadcastText && packet.Header.Recipient == ClietnId)) && packet.Header.Option1 != PacketOption1.Acknowledge)
                                {
                                    ParsePacket(packet);
                                }
                                continue;
                            }
                            //Функция разбора пакета
                            ParsePacket(packet);
                        }

                    }
                    else
                    {
                        // Проверка crc и id клиента, то есть предназначен ли этот пакет этому клиенту или пакет является широковешательным
                        // if (packet.Header.Crc == crc && (packet.Header.Recipient == ClietnId || packet.Data.Type == DataType.BroadcastText )) // // До обновления бродкастинга
                        if (packet.Header.Crc == crc && packet.Header.Recipient == ClietnId)
                        {
                            //Функция разбора пакета
                            ParsePacket(packet);
                        }
                        else
                        {

                            // MessageBox.Show("Hash or ID NOT matches!");
                            // MessageBox.Show(Packet.Packet<Data>Info());
                        }   
                    }

                    
                    
                }

                catch (InvalidOperationException)
                {
                    IsPortAvailable(readPort);
                    // Передает событие с текстом ошибки
                  //  OnMessageRecived(new MessageRecivedEventArgs(MessageType.ReadPortUnavailable,  _comPortReader.PortName, 255));
                  //  ReOpenPort(_comPortReader);
                    Thread.Sleep(3000);
                }

                catch (Exception exception)
                {

                }

            }
        }

       private void ParsePacket(Packet.Packet packet)
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
                        var text = StructConvertor.FromBytes<Text>(packet.Data.Content);

                        // Определяет необходимость разархивирования
                        if (packet.Header.Option2 == PacketOption2.Compressed)
                        {
                            text.Content = Compressor.Unzip(packet.Data.Content);
                        }

                        // Событие передает тип пакета и текст для пользователя
                        OnMessageRecived(new MessageRecivedEventArgs(MessageType.Text, Encoding.UTF8.GetString(text.Content), packet.Header.Sender, packet.Header.Recipient));

                        // Выслать ACK
                        SendAcknowledge(packet);
                    }
                    break;

                // Обработка пакета текстового сообщения 
                case DataType.BroadcastText:
                    {
                        var broabroadcastText = StructConvertor.FromBytes<BroadcastText>(packet.Data.Content);

                        // Определяет необходимость разархивирования
                        if (packet.Header.Option2 == PacketOption2.Compressed)
                        {
                            broabroadcastText.Content = Compressor.Unzip(packet.Data.Content);
                        }

                        // Событие передает тип пакета и текст для пользователя
                        OnMessageRecived(new MessageRecivedEventArgs(MessageType.BroadcastText, Encoding.UTF8.GetString(broabroadcastText.Content), packet.Header.Sender, packet.Header.Recipient));

                        // Выслать ACK
                        SendAcknowledge(packet);
                    }
                    break;

                // Обработка пакета запроса на передачу файла
                case DataType.FileRequest:
                    {
                        var fileRequest = StructConvertor.FromBytes<FileRequest>(packet.Data.Content);

                            // Выслать подверждение получения пакета
                            SendAcknowledge(packet);

                            // Если не принимается и не передается файл
                            if (!Client.IsSendingFile && !IsRecivingFile)
                            {
                                var ea = new FileRequestRecivedEventArgs(fileRequest.FileName, fileRequest.FileLenght, packet.Header.Sender);

                                OnFileRequestRecived(ea);

                                _waitFileTransferAnswer = true;

                                // Сохраняет параметры файла
                                _receivingFileName = fileRequest.FileName;
                                ReceivingFileSize = fileRequest.FileLenght;
                                // Сохраняет id отправителя файла
                                _fileSender = packet.Header.Sender;

                                _waitFileTransferAnswerTimer.Change(_waitFileTransferAnswerTimeOut, Timeout.Infinite);
                            }
                            else
                            {
                                SendFileTransferCancel(packet.Header.Sender);

                                // Событие - сообщение о ошибке 
                                OnMessageRecived(new MessageRecivedEventArgs(MessageType.Error,
                                                                             "Клиенту № " + packet.Header.Sender +
                                                                             " Было отказано в передачи файла "
                                                                             + fileRequest.FileName + " размером " +
                                                                             fileRequest.FileLenght / 1024 / 1024
                                                                             +
                                                                             "МБ, так как в данный момент уже осуществляется прием другого файла ",
                                                                             0, 0));
                            }
                    }
                        break;

                    // Обработка пакета файла
                    case DataType.FileData :
                    {
                        var fileData = StructConvertor.FromBytes<FileData>(packet.Data.Content);
                        
                        // Если совпал номер пакета и разрешено получение файлов и файл существует и id отправителя совпал с id отправителя файла
                            if (Client.CountOfFilePackets == fileData.PacketNumber && IsRecivingFile &&
                            File.Exists(ReceivingFileFullName) && packet.Header.Sender == _fileSender)
                        {
                            ProcessedFileSize += _filePacketSize;
                            // Устанавливает что пакет получен
                            _waitForFilePacketEvent.Set();
                            // Выслать подверждение получения пакета
                            SendAcknowledge(packet);

                            // Разархивация данных
                          //  fileData.Content = Compressor.Unzip(packet.Data.Content);
                           // var unzipContent = Compressor.Unzip(packet.Data.Content);

                            // Запись данных в файл
                            if (!ByteArrayToFile(ReceivingFileFullName, fileData.Content))
                            {
                                OnMessageRecived(new MessageRecivedEventArgs(MessageType.Error,
                                                                             "Ошибка при записи в файл",
                                                                             packet.Header.Sender, 0));
                                OnMessageRecived(
                                    new MessageRecivedEventArgs(MessageType.FileTransferCanceledRecipientSide,
                                                                _receivingFileName, _fileSender, 0));
                                CancelRecivingFile();
                                break;
                            }



                            // Инкрементирует число принятых пакетов
                            Client.CountOfFilePackets++;
                            // Если пакет последний в цепочке
                            if (fileData.LastPacket)
                            {
                                // Событие о заверщении приема файла 
                                OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileReceivingComplete,
                                                                             _receivingFileName, packet.Header.Sender,
                                                                             packet.Header.Recipient));
                                SendFileTransferCompleted(packet.Header.Sender);
                                // Закрывает поток записи файла
                                _fileStream.Close();
                                IsRecivingFile = false;
                            }
                        }
                        else
                        {
#if DEBUG
                            // Событие - сообщение о ошибке 
                            OnMessageRecived(new MessageRecivedEventArgs(MessageType.Error,
                                                                         "Пакет файла: не совпал номер пакета, принятый номер " +
                                                                         fileData.PacketNumber +
                                                                         "сохраненый номер " + Client.CountOfFilePackets,
                                                                         packet.Header.Sender, 0));
#endif
                        }
                    
            }
                        break;
            }
        }

       private bool ParseOptions(Packet.Packet packet)
        {
           switch (packet.Header.Option1)
           {
               case PacketOption1.Acknowledge:
                   {

                       var acknowledge = StructConvertor.FromBytes<Acknowledge>(packet.Data.Content);
                       // Обработка пакета подверждения доставки сообщения
                       // Если первый бит опций равен ACK и CRC в пакете совпала с последним отправленым пакетом для этого клиента
                      // if (_clientArray[packet.Header.Sender].LastMessageCrc == BitConverter.ToUInt16(packet.ByteData, 0))
                       if (_clientArray[packet.Header.Sender].LastMessageCrc == acknowledge.DeliveredPacketCrc)
                       {
                           // Елси был доставлен пакет "запрос на передачу файла" то запустить таймер ожидания ответа
                           if (_clientArray[packet.Header.Sender].LastPacket.Data.Type == DataType.FileRequest)
                           {
                               OnMessageRecived(new MessageRecivedEventArgs(MessageType.WaitFileRecipientAnswer, FileToTransfer.Name, packet.Header.Sender, packet.Header.Recipient));
                               _waitFileTransferAnswerTimer.Change(_waitFileTransferAnswerTimeOut, Timeout.Infinite);
                           }

                           //  Если последний пакет данных успешно доставлен то увеличить количество переданных байт на величину пакета
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
                           OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileTransferAllowed, FileToTransfer.Name, packet.Header.Sender, packet.Header.Recipient));

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
                           OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileTransferCanceledBySender, _receivingFileName, packet.Header.Sender, packet.Header.Recipient));
                           
                           CancelRecivingFile();
                           // Выслать подверждение получения пакета
                           SendAcknowledge(packet);
                           return true;
                       }
                       // Если файл отправлялся
                       if (Client.IsSendingFile)
                       {
                           // Событие отмены перадачи файла получателем
                           OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileTransferCanceledByRecipient, FileToTransfer.Name, packet.Header.Sender, packet.Header.Recipient));
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
                           OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileTransferDenied, FileToTransfer.Name, packet.Header.Sender, packet.Header.Recipient));
                       }

                       if (_waitFileTransferAnswer)
                       {
                           _waitFileTransferAnswer = false;
                           // Отправитель отменил запрос
                           OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileRequestCanceledRecipientSide, _receivingFileName, packet.Header.Sender, packet.Header.Recipient));
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
                           OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileSendingComplete, FileToTransfer.Name, packet.Header.Sender, packet.Header.Recipient));
                           SendAcknowledge(packet);
                           return true;
                       }
                   }
                   break;
           }
           return false;
        }

       public bool CreateFile (ref string fileName)
       {
            string FileName = fileName;
            // Создает папку в "Мои документы" если ее нет
            string md = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\ChatRecivedFiles\\";
            if (Directory.Exists(md ) == false)
            {
                Directory.CreateDirectory(md);
            }

           if ((File.Exists(md+fileName)))
           {
               string justName = Path.GetFileNameWithoutExtension(md + fileName);
               string fileExtinsion = Path.GetExtension(md + fileName);

               int i = 0;
               while ((File.Exists(md + FileName)))
               {
                   FileName = String.Format(justName + "({0})" + fileExtinsion, ++i);
               }

               fileName = FileName;

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
                FileStream fs = File.Create(md + FileName);
                ReceivingFileFullName = md + FileName;
                fs.Close();
                return true;
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
                // Закрывает поток записи файла
                _fileStream.Close();
                // Событие - прием файла отклонен
                OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileTransferCanceledRecipientSide, _receivingFileName, _fileSender, ClietnId));
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
                _clientArray[Client.FileRecipient].OutFilePacketsQueue = new ConcurrentQueue<Packet.Packet>();
                // Высылает уведемление о прекращении передачи файла
                SendFileTransferCancel(Client.FileRecipient);
                // Обнуляет счетчик
                Client.CountOfFilePackets = 0;
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Событие - передача файла отменена
                OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileTransferCanceledSenderSide, FileToTransfer.Name, Client.FileRecipient, 0));
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
               OnMessageRecived(new MessageRecivedEventArgs(MessageType.FileRequestCanceledSenderSide, FileToTransfer.Name, Client.FileRecipient, 0));

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
                        // TODO: do something
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
                    // Записывает блок байтов в поток и следовательно в файл
                    _fileStream.Write(byteArray, 0, byteArray.Length); 
                }
                return true;
            }
            catch (Exception _Exception)
            {
                // Ошибка
              //  Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());

                OnMessageRecived(new MessageRecivedEventArgs(MessageType.Error, _Exception.ToString(), _fileSender, 0));
            }

            // В случае ошибки возвращает false
            return false;
        }

       public static bool ReOpenPort(SerialPort port)
        {
            try
            {
                if (port.IsOpen)
                {
                    port.Close();
                }
                
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
