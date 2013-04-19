using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Text;
using System.Threading;
using ChatClient;
using ChatClient.Main;
using ChatClient.Main.Packet;
using ChatClient.Main.Packet.DataTypes;

namespace Chat.Main
{
    class Client
    {
        public byte IdToSend { get; private set; }
        public ManualResetEvent AnswerEvent { get; private set; }
        public ConcurrentQueue<Packet> OutMessagesQueue { get; private set; }
        public ConcurrentQueue<Packet> OutFilePacketsQueue { get;  set; }
        private Thread _writeThread;
        private int _sleepTime = 1;
        public static bool Continue = false;
        public ushort LastMessageCrc { get; private set; }
        public Packet LastPacket { get; private set; }
        private SerialPort _comPortWriter;
        public Queue InputMessageQueue;
        public static bool IsSendingFile = false;
        public static bool AllowSendingFile = false;
        public static byte CountOfFilePackets = 0;
        private byte _ownerId;
        public static byte FileRecipient = 0;
        public int QueueSize { get; private set; }

        public Client (byte idTosend,byte ownerId, int queueSize, SerialPort writePort)
        {
            IdToSend = idTosend;
            _ownerId = ownerId;
            QueueSize = queueSize;
            AnswerEvent = new ManualResetEvent(false);
            OutMessagesQueue = new ConcurrentQueue<Packet>();
            OutFilePacketsQueue = new ConcurrentQueue<Packet>();
            Continue = true;
            _writeThread = new Thread(Write);
            _writeThread.Start();
            LastMessageCrc = 0;
            _comPortWriter = writePort;
        }

        public event EventHandler<MessageRecivedEventArgs> AcknowledgeRecived;

        private void OnAcknowledgeRecived(MessageRecivedEventArgs e)
        {
            EventHandler<MessageRecivedEventArgs> handler = AcknowledgeRecived;
            if (handler != null) handler(this, e);
        }

        public bool SendPacketNow (Packet packet)
        {
            if (!TryWrite(_comPortWriter, packet))
            {
                // Передает событие с текстом ошибки
               // OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.Error, "Порт " + _comPortWriter.PortName + " недоступен, отправка невозможна.", 0));
                return false;
            }
            return true;
        }

        public bool AddPacketToQueue(byte[] messageBody, byte sender, byte option1 = 0x00, byte option2 = 0x00, bool sendPacketImmediately = false)
        {
            Packet packet = new Packet(new Header(IdToSend, sender, option1, option2), messageBody);

            // TO DO: Поправить логику условий
            // Валидация данных
            if (sendPacketImmediately)
            {
                if (!TryWrite(_comPortWriter, packet))
                {
                    // Передает событие с текстом ошибки
                    OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.Error, "Порт " + _comPortWriter.PortName + " недоступен, отправка невозможна.", 0));
                } 
                return true;
            }

            if (packet.Data.Type == DataType.FileData)
            {
                OutFilePacketsQueue.Enqueue(packet);
                return true;
            }

            //Добавляем пакет в очередь на отправку
            OutMessagesQueue.Enqueue(packet);
            return true;
        }

        private void Write()
        {
            Packet outPacket;
            while (Continue)
            {
                Thread.Sleep(_sleepTime*100);

                // Проверка пакетов в очереди
                while (OutMessagesQueue.TryDequeue(out outPacket) || OutFilePacketsQueue.TryDequeue(out outPacket))
                {
                    AnswerEvent.Reset();

                    LastMessageCrc = Crc16.ComputeChecksum(outPacket.ByteData);
                    LastPacket = outPacket;   

                    if (!TryWrite(_comPortWriter, outPacket))
                    {
                        // Событие о недостцуппности порта
                        OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.WritePortUnavailable, _comPortWriter.PortName, outPacket.Header.Recipient));
                        // Событие о недоставки сообщения
                        OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.TextUndelivered, Encoding.UTF8.GetString(outPacket.Data.Content), outPacket.Header.Recipient));
                        continue;
                    }
                    else
                    {
                        if (outPacket.Data.Type == DataType.Text)
                        {
                            OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.WritePortAvailable, _comPortWriter.PortName, outPacket.Header.Recipient));
                        }
                    }
                    
                    byte attempts = 0;
                    while (true)
                    {
                        if (AnswerEvent.WaitOne(3000, false))
                        {
                            if (outPacket.Data.Type == DataType.Text)
                            {
                                // События получения Acknowledge, передает тип, текст отправленного сообщения и получателя сообщения   ORIGINAL 
                                  OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.TextDelivered, Encoding.UTF8.GetString(outPacket.Data.Content), outPacket.Header.Recipient));
                               
                                // TEST
                                //OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.TextDelivered, Encoding.UTF8.GetString(outPacket.Data.Content), outPacket.Header.Sender));
                            }
                            break;
                        }
                        else
                        {
                            if (outPacket.Data.Type ==  DataType.FileData && !IsSendingFile)
                            {
                                CancelSendingFile();
                                break;
                            }
                            if (++attempts > 3)
                            {
                                if (outPacket.Data.Type ==  DataType.FileData)
                                {
                                    CancelSendingFile();
                                    OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.FileUndelivered, "Получатель не доступен доставка файла отменена", outPacket.Header.Recipient));
                                }

                                if (outPacket.Data.Type ==  DataType.Text)
                                {
                                    OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.TextUndelivered, Encoding.UTF8.GetString(outPacket.Data.Content), outPacket.Header.Recipient));
                                }
                                else
                                {
                                    OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.MessageUndelivered, Encoding.UTF8.GetString(outPacket.Data.Content), outPacket.Header.Recipient));
                                }
                                break;
                            }

#if DEBUG
                            // Debug message
                            OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.Resend, Encoding.UTF8.GetString(outPacket.Data.Content), outPacket.Header.Recipient));
#endif
                            if (!TryWrite(_comPortWriter, outPacket))
                            {
                                // Передает событие с текстом ошибки
                                OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.WritePortUnavailable, _comPortWriter.PortName, outPacket.Header.Recipient));
                                OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.TextUndelivered, Encoding.UTF8.GetString(outPacket.Data.Content), outPacket.Header.Recipient));
                                if (outPacket.Data.Type == DataType.FileData)
                                {
                                    CancelSendingFile();
                                    OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.FileUndelivered, "Получатель не доступен доставка файла отменена", outPacket.Header.Recipient));
                                }
                                break;
                            }
                            OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.WritePortAvailable, _comPortWriter.PortName, outPacket.Header.Recipient));

                        }
                    } 
                }
            }
        }

        private void SendFileTransferCancel(byte toId)
        {
            AddPacketToQueue(new byte[] { 0x00 }, _ownerId, 0x18);
        }

        public void CancelSendingFile()
        {
            if (Client.IsSendingFile)
            {
                // Устанавливает что файл не передается
                IsSendingFile = false;
                // Запрещает передавать файла до запроса на отправку
                AllowSendingFile = false;
                // Отчищает очередь на отправку файла
                OutFilePacketsQueue = new ConcurrentQueue<Packet>();
                // Высылает уведемление о прекращении передачи файла
                SendFileTransferCancel(FileRecipient);
                // Обнуляет счетчик
                CountOfFilePackets = 0;
            }
        }

        private bool TryWrite(SerialPort port, Packet packet)
        {
                try
                {
                    lock (port)
                    {
                        port.Write(packet.ToByte(), 0, packet.ToByte().Length);
                    }
                    return true;
                }
                catch (Exception)
                {
                    ClientPort.IsPortAvailable(port);
                    ClientPort.ReOpenPort(port);
                    return false;
                }
            return false;
        }
    }
}
