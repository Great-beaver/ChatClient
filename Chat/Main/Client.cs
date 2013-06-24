using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Text;
using System.Threading;
using Chat.Helpers;
using Chat.Main.Packet;
using Chat.Main.Packet.DataStructs;
using Chat.Main.Packet.DataTypes;

namespace Chat.Main
{
    class Client
    {
        public byte Id { get; private set; }
        public byte OwnerId { get; private set; }
        public ManualResetEvent AnswerEvent { get; private set; }
        public ConcurrentQueue<Packet.Packet> OutMessagesQueue { get; private set; }
        public ConcurrentQueue<Packet.Packet> OutFilePacketsQueue { get; set; }
        private Thread _writeThread;
        protected int _sleepTime = 1;
        public static bool Continue = false;
        public ushort LastMessageCrc { get; protected set; }
        public Packet.Packet LastPacket { get; protected set; }
        protected SerialPort ComPortWriter;
        public Queue InputMessageQueue;
        public static bool IsSendingFile = false;
        public static bool AllowSendingFile = false;
        public static byte CountOfFilePackets = 0;
        
        public static byte FileRecipient = 0;
        public int QueueSize { get; private set; }

        /// <summary>
        /// Создает новый объект для передачи пакетов определеному клиенту
        /// </summary>
        /// <param name="idTosend">ID клиента которому будут передаватся пакеты</param>
        /// <param name="ownerId">ID владельца объекта, использоуется в каждом пакете для определения отправителя, на этот адрес будут возвращатся подтверждения доставки пакета</param>
        /// <param name="queueSize">Размер очереди пакетов</param>
        /// <param name="writePort">Порт в который будет производится запись пакетов</param>
        public Client (byte idTosend,byte ownerId, int queueSize, SerialPort writePort)
        {
            Id = idTosend;
            OwnerId = ownerId;
            QueueSize = queueSize;
            AnswerEvent = new ManualResetEvent(false);
            OutMessagesQueue = new ConcurrentQueue<Packet.Packet>();
            OutFilePacketsQueue = new ConcurrentQueue<Packet.Packet>();
            Continue = true;
            _writeThread = new Thread(Write);
            _writeThread.Start();
            LastMessageCrc = 0;
            ComPortWriter = writePort;
        }

        public event EventHandler<MessageRecivedEventArgs> AcknowledgeRecived;

        protected void OnAcknowledgeRecived(MessageRecivedEventArgs e)
        {
            EventHandler<MessageRecivedEventArgs> handler = AcknowledgeRecived;
            if (handler != null) handler(this, e);
        }

       


        public bool SendPacketNow(Packet.Packet packet)
        {
            if (!TryWrite(ComPortWriter, packet))
            {
                // Передает событие с текстом ошибки
               // OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.Error, "Порт " + _comPortWriter.PortName + " недоступен, отправка невозможна.", 0));
                return false;
            }
            return true;
        }

        public bool AddPacketToQueue(object data, byte sender, byte option1 = 0x00, byte option2 = 0x00, bool sendPacketImmediately = false)
        {
            Packet.Packet packet = new Packet.Packet(new Header(Id, sender, option1, option2), data);

            // TO DO: Поправить логику условий
            // Валидация данных
            if (sendPacketImmediately)
            {
                if (!TryWrite(ComPortWriter, packet))
                {
                    // Передает событие с текстом ошибки
                    OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.Error, "Порт " + ComPortWriter.PortName + " недоступен, отправка невозможна.", 0,0));
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

        protected virtual void Write()
        {
            Packet.Packet outPacket;
            while (Continue)
            {
                Thread.Sleep(_sleepTime*100);

                // Проверка пакетов в очереди
                while (OutMessagesQueue.TryDequeue(out outPacket) || OutFilePacketsQueue.TryDequeue(out outPacket))
                {
                    AnswerEvent.Reset();
                    LastMessageCrc = Crc16.ComputeChecksum(outPacket.ByteData);
                    LastPacket = outPacket;   

                    if (!TryWrite(ComPortWriter, outPacket))
                    {
                        // Событие о недоступпности порта
                        OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.WritePortUnavailable, ComPortWriter.PortName, outPacket.Header.Recipient,0));
                        // Событие о недоставки сообщения
                        OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.TextUndelivered, Encoding.UTF8.GetString(outPacket.Data.Content), outPacket.Header.Recipient,0));
                        continue;
                    }
                    else
                    {
                        if (outPacket.Data.Type == DataType.Text)
                        {
                            OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.WritePortAvailable, ComPortWriter.PortName, outPacket.Header.Recipient,0));
                        }
                    }
                    
                    byte attempts = 0;
                    while (true)
                    {
                        // Ожидание отчета о доставке 
                        if (AnswerEvent.WaitOne(3000, false))
                        {
                            // Посылает отчет о доставке текстовых пакетов
                            if ((outPacket.Data.Type == DataType.Text))
                            {
                                // TODO: Исправить вызов OnAcknowledgeRecived, как Sender передается Recipient
                                //TODO: Возвращать не Content а десериализованный текст 

                                var text = StructConvertor.FromBytes<Text>(outPacket.Data.Content);


                                // События получения Acknowledge, передает тип, текст отправленного сообщения и получателя сообщения   ORIGINAL 
                                OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.TextDelivered, Encoding.UTF8.GetString(text.Content), outPacket.Header.Recipient, 0));
                            }

                            // Если доставлен широковещательный пакет 
                            if (outPacket.Data.Type == DataType.BroadcastText)
                            {
                                //TODO: Возвращать не Content а десериализованный текст 
                                var broadcastText = StructConvertor.FromBytes<BroadcastText>(outPacket.Data.Content);
                                // Генерация события о доставке
                                OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.BroadcastTextDelivered, Encoding.UTF8.GetString(broadcastText.Content), outPacket.Header.Sender, outPacket.Header.Recipient));
                            }

                            break;
                        }
                        else // Если вышел таймаут отправки сообещния 
                        {
                            // Если отправлялся файл и за время отпраавки пакета отправка файла была отменена то отменяется отправка файла и прекращаются дальнейшие попытки отправки 
                            if (outPacket.Data.Type ==  DataType.FileData && !IsSendingFile)
                            {
                                CancelSendingFile();
                                break;
                            }
                            // Если количество попыток переотправки Больше 3 то прекращаются попытки переотправки и пользователю высылается уведомление о не доставке 
                            if (++attempts > 3)
                            {
                                // Если не доставлен пакет файла 
                                if (outPacket.Data.Type ==  DataType.FileData)
                                {
                                    CancelSendingFile();
                                    OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.FileUndelivered, "Получатель не доступен доставка файла отменена", outPacket.Header.Recipient,0));
                                    break;
                                }

                                // Если не доставлен широковещательный пакет
                                if (outPacket.Data.Type == DataType.BroadcastText)
                                {
                                    //TODO: Возвращать не Content а десериализованный текст 
                                    var broadcastText = StructConvertor.FromBytes<BroadcastText>(outPacket.Data.Content);
                                    // Генерация события о не доставке
                                    OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.BroadcastTextUndelivered, Encoding.UTF8.GetString(broadcastText.Content), outPacket.Header.Sender, outPacket.Header.Recipient));
                                    break;
                                }

                                // Еслит не доставлен текстовый пакет
                                if ((outPacket.Data.Type == DataType.Text))
                                {
                                    //TODO: Возвращать не Content а десериализованный текст 
                                    var text = StructConvertor.FromBytes<Text>(outPacket.Data.Content);
                                    OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.TextUndelivered, Encoding.UTF8.GetString(text.Content), outPacket.Header.Recipient, 0));
                                    break;
                                }
                                    
                                // Если пакет не подходит не под один критерий
                                OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.MessageUndelivered, Encoding.UTF8.GetString(outPacket.Data.Content), outPacket.Header.Recipient,0));
                                    
                                break;
                            }
                            // иначе продолжаются попытки переотправки пакета
#if DEBUG
                            OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.Resend, Encoding.UTF8.GetString(outPacket.Data.Content), outPacket.Header.Recipient,0));
#endif
                            if (!TryWrite(ComPortWriter, outPacket))
                            {
                                // Передает событие с текстом ошибки
                                OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.WritePortUnavailable, ComPortWriter.PortName, outPacket.Header.Recipient,0));
                                OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.TextUndelivered, Encoding.UTF8.GetString(outPacket.Data.Content), outPacket.Header.Recipient,0));
                                if (outPacket.Data.Type == DataType.FileData)
                                {
                                    CancelSendingFile();
                                    OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.FileUndelivered, "Получатель не доступен доставка файла отменена", outPacket.Header.Recipient,0));
                                }
                                break;
                            }
                            // Уведомляет полтьзователя о доступности порта записи
                            OnAcknowledgeRecived(new MessageRecivedEventArgs(MessageType.WritePortAvailable, ComPortWriter.PortName, outPacket.Header.Recipient,0));

                        }
                    } 
                }
            }
        }

        private void SendFileTransferCancel(byte toId)
        {
            AddPacketToQueue(new byte[] { 0x00 }, OwnerId, 0x18);
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
                OutFilePacketsQueue = new ConcurrentQueue<Packet.Packet>();
                // Высылает уведемление о прекращении передачи файла
                SendFileTransferCancel(FileRecipient);
                // Обнуляет счетчик
                CountOfFilePackets = 0;
            }
        }

        public static bool TryWrite(SerialPort port, Packet.Packet packet)
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
                    CommunicationUnit.IsPortAvailable(port);
                    CommunicationUnit.ReOpenPort(port);
                    return false;
                }
            return false;
        }
    }
}
