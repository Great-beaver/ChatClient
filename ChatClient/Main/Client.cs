using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatClient.Main
{
    class Client
    {
        public byte IdToSend { get; private set; }
        public ManualResetEvent AnswerEvent { get; private set; }
        public Queue OutMessagesQueue { get; private set; }
        public  Queue OutFilePacketsQueue { get; private set; }
        private Thread _writeThread;
        private int _sleepTime = 1;
        public static bool Continue = false;
        public ushort LastMessageCrc { get; private set; }
        public Packet.Packet LastPacket { get; private set; }
        private SerialPort _comPortWriter;
        public Queue InputMessageQueue;
        public static bool IsSendingFile = false;
        public static bool AllowSendingFile = false;
        public static byte CountOfFilePackets = 0;
        private byte _ownerId;
        public static byte FileRecipient = 0;
        public int QueueSize { get; private set; }

        public Client (byte idTosend,byte ownerId, int queueSize, SerialPort writePort, Queue inputQueue)
        {
            IdToSend = idTosend;
            _ownerId = ownerId;
            QueueSize = queueSize;
            AnswerEvent = new ManualResetEvent(false);
            OutMessagesQueue = new Queue(QueueSize);
            OutFilePacketsQueue = new Queue(QueueSize); ;
            Continue = true;
            _writeThread = new Thread(Write);
            _writeThread.Start();
            LastMessageCrc = 0;
            _comPortWriter = writePort;
            InputMessageQueue = inputQueue;
        }

        public event EventHandler<MessageRecivedEventArgs> AcknowledgeRecived;

        private void OnAcknowledgeRecived(MessageRecivedEventArgs e)
        {
            EventHandler<MessageRecivedEventArgs> handler = AcknowledgeRecived;
            if (handler != null) handler(this, e);
        }


        public bool AddPacketToQueue(byte[] messageBody, byte sender, byte option1 = 0x00, byte option2 = 0x00, bool sendPacketImmediately = false)
        {
            Packet.Packet packet = new Packet.Packet(IdToSend, sender, option1, option2, messageBody);

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
                lock (OutFilePacketsQueue)
                {
                    // _outFilePacketsQueue.Enqueue(packet.ToByte());
                    OutFilePacketsQueue.Enqueue(packet);
                }
                return true;
            }

            //Добавляем пакет в очередь на отправку
            lock (OutMessagesQueue)
            {
                // _outMessagesQueue.Enqueue(packet.ToByte());
                OutMessagesQueue.Enqueue(packet);
            }

            return true;
        }

        private void Write()
        {
            // byte[] outPacket = new byte[0];
            Packet.Packet outPacket;
            while (Continue)
            {
                Thread.Sleep(_sleepTime);

                // Определяет есть ли пакеты в очереди
                if (QueueCount(OutMessagesQueue) > 0)
                {
                    // Блокировка очередни на время извлечения пакета
                    lock (OutMessagesQueue)
                    {
                        outPacket = (Packet.Packet)OutMessagesQueue.Dequeue();
                    }
                }
                else
                {
                    // Определяет есть ли пакеты файлов в очереди и разрешена передача файла 
                    if (QueueCount(OutFilePacketsQueue) > 0 && IsSendingFile)
                    {
                        // Блокировка очередни на время извлечения пакета
                        lock (OutFilePacketsQueue)
                        {
                            outPacket = (Packet.Packet)OutFilePacketsQueue.Dequeue();
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                AnswerEvent.Reset();

                 //Сохраняет CRC последнего отправленого сообщения, для последующей проверки получения сообщения
                 //byte[] data= new byte[outPacket.Length-10];

                 // Array.Copy(outPacket,10,data,0,data.Length);

                LastMessageCrc = Crc16.ComputeChecksum(outPacket.ByteData);
                LastPacket = outPacket;

                lock (_comPortWriter)
                {
                    _comPortWriter.Write(outPacket.ToByte(), 0, outPacket.ToByte().Length);
                }

                byte attempts = 0;

                while (true)
                {
                    if (AnswerEvent.WaitOne(3000, false))
                    {
                        if (outPacket.Data.Type == "Text")
                        {
                          //lock (InputMessageQueue)
                          //{
                          //    InputMessageQueue.Enqueue("Сообщение доставлено!");
                          //}
                            // События получения Acknowledge, передает тип, текст отправленного сообщения и получателя сообщения
                            OnAcknowledgeRecived(new MessageRecivedEventArgs("ACK", Encoding.UTF8.GetString(outPacket.Data.Content), outPacket.Recipient));

                        }
                        break;
                    }
                    else
                    {
                        if (outPacket.Data.Type == "FileData" && !IsSendingFile)
                        {
                            CancelSendingFile();
                            break;
                        }

                        if (++attempts > 3)
                        {
                            if (outPacket.Data.Type == "FileData")
                            {
                                CancelSendingFile();
                                OnAcknowledgeRecived(new MessageRecivedEventArgs("FileUndelivered", "Получатель не доступен доставка файла отменена", outPacket.Recipient));
//#if DEBUG
//                                MessageBox.Show("Получатель не доступен доставка файла отменена");
//#endif
                            }

                            if (outPacket.Data.Type == "Text")
                            {
                                OnAcknowledgeRecived(new MessageRecivedEventArgs("TextUndelivered", Encoding.UTF8.GetString(outPacket.Data.Content), outPacket.Recipient));
                            }

                            else
                            {
                                OnAcknowledgeRecived(new MessageRecivedEventArgs("MessageUndelivered", Encoding.UTF8.GetString(outPacket.Data.Content), outPacket.Recipient));
                            }

                            // lock (InputMessageQueue)
                          // {
                          //     InputMessageQueue.Enqueue("Сообщение НЕ доставлено!");
                          // }
                            
                            break;
                        }
                       
#if DEBUG
                        // Debug message
                        MessageBox.Show("Переотправка сообщения попытка № " + attempts);
#endif

                        lock (_comPortWriter)
                        {
                            _comPortWriter.Write(outPacket.ToByte(), 0, outPacket.ToByte().Length);
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
                // Ждет завершения потока что бы он не слал больше пакетов. Надо ли оно тут?
                //_fileSenderThread.Join(1000);
                // Отчищает очередь на отправку файла
                OutFilePacketsQueue.Clear();
                // Высылает уведемление о прекращении передачи файла
                SendFileTransferCancel(FileRecipient);
                // Обнуляет счетчик
                CountOfFilePackets = 0;
            }
        }

        private int QueueCount(Queue queue)
        {
            lock (queue)
            {
                return queue.Count;
            }

        }

    }
}
