using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chat.Main;
using ChatClient.Main;

namespace ChatServer
{
    public partial class Form1 : Form
    {
        private ServerPort _comPort;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           _comPort = new ServerPort(Properties.Settings.Default.readerPortName1,Properties.Settings.Default.readerPortName2,Properties.Settings.Default.readerPortName3,
               Properties.Settings.Default.readerPortName4,Properties.Settings.Default.WriteComPort,Properties.Settings.Default.ClientId,Properties.Settings.Default.ComPortSpeed);
            // Подписывание на событие
              _comPort.MessageRecived += new EventHandler<MessageRecivedEventArgs>(ComPortMessageRecived);
              _comPort.FileRequestRecived += new EventHandler<FileRequestRecivedEventArgs>(ComPortFileRequestRecived);  
        }

        // Делегат обработчика  текстовых сообщений 
        public delegate void ReciveMessageDelegate(MessageType type, string text, byte sender);

        // Метод обработки текстовых сообщений 
        void ReciveMessage(MessageType type, string text, byte sender)
        {
            switch (type)
            {
                case MessageType.Text:
                    {
                        richTextBox1.AppendText("Клиент " + sender + " :" + '\n' + text + '\n');
                    }
                    break;

                case MessageType.TextDelivered:
                    {
                        richTextBox1.AppendText(text + '\n' +
                    "Сообщение для клиента " + sender + " доставлено." + '\n');
                    }
                    break;

                case MessageType.TextUndelivered:
                    {
                        richTextBox1.AppendText(text + '\n' +
                    "Сообщение для клиента " + sender + "НЕ доставлено." + '\n');
                    }
                    break;

                case MessageType.FileUndelivered:
                    {
                        richTextBox1.AppendText(
                    "Получатель " + sender + "не доступен, отправка файла отменена." + '\n');
                    }
                    break;

                case MessageType.MessageUndelivered:
                    {
                        richTextBox1.AppendText("Клиента " + sender + "не доступен." + '\n');
                    }
                    break;

                case MessageType.FileReceivingComplete:
                    {
                        richTextBox1.AppendText(
                    "Файл " + text + " от клиента " + sender + " получен." + '\n');
                    }
                    break;

                case MessageType.FileSendingComplete:
                    {
                        richTextBox1.AppendText(
                    "Передача файла " + text + " клиенту " + sender + " завершена." + '\n');
                    }
                    break;

                case MessageType.FileTransferAllowed:
                    {
                        richTextBox1.AppendText(
                    "Клиент " + sender + " одобрил получение файла " + text + "." + '\n');
                    }
                    break;

                case MessageType.FileTransferDenied:
                    {
                        richTextBox1.AppendText(
                    "Клиент " + sender + " отклонил получение файла " + text + "." + '\n');
                    }
                    break;

                case MessageType.FileReceivingStarted:
                    {
                        richTextBox1.AppendText(
                        "Начат прием файла  " + text + " от клиента " + sender + "." + '\n');
                    }
                    break;

                case MessageType.FileTransferCanceled:
                    {
                        richTextBox1.AppendText(
                        "Прием файла  " + text + " от клиента " + sender + " отменен." + '\n');
                    }
                    break;
                case MessageType.FileTransferCanceledBySender:
                    {
                        richTextBox1.AppendText(
                    "Передача файла " + text + " отменан отправителем " + sender + "." + '\n');
                    }
                    break;

                case MessageType.FileTransferCanceledByRecipient:
                    {
                        richTextBox1.AppendText(
                    "Передача файла " + text + " отменан получателем " + sender + "." + '\n');
                    }
                    break;

                case MessageType.FileReceivingTimeOut:
                    {
                        richTextBox1.AppendText(
                    "Вышло время ожидания файла " + text + " от " + sender + " передача отменена." + '\n');
                    }
                    break;

                case MessageType.Error:
                    {
                        richTextBox1.AppendText(text + '\n');
                    }
                    break;

            }

            // Скрол вниз
            if (richTextBox1.TextLength > 0)
                richTextBox1.Select(this.richTextBox1.TextLength - 1, 0);
            richTextBox1.ScrollToCaret();

        }

        // Получение данных и вызов обработчика для текстовых сообщений
        void ComPortMessageRecived(object sender, MessageRecivedEventArgs e)
        {
            try
            {
                BeginInvoke(new ChatClient.Form1.ReciveMessageDelegate(ReciveMessage), e.MessageType, e.MessageText, e.Sender);
            }
            catch (Exception)
            {
                // TO DO: Do something  
            }

        }

        // Делегат обработчика запроса на передачу
        public delegate void ReciveFileRequestDelegate(FileRequestRecivedEventArgs e);

        // Метод обработки запроса на передачу
        void ReciveFileRequest(FileRequestRecivedEventArgs e)
        {
            //  MessageBox.Show("Запрос на передачу файла " + fileName + " размером " + fileLenght / 1024 / 1024 + "МБ" + " от клиента " + sender);

            // Сообщения о том надо ли принимать файл
            DialogResult dialogResult = MessageBox.Show("Прниять файл " + e.FileName +
                   "от клиента № " + e.Sender + " размером " + e.FileLenght / 1024 / 1024 + "МБ?",
                   "Передача файла", MessageBoxButtons.YesNo);

            e.FileTransferAllowed = dialogResult == DialogResult.Yes;
        }

        // Получение данных и вызов обработчика запроса на передачу
        void ComPortFileRequestRecived(object sender, FileRequestRecivedEventArgs e)
        {
            // ivoke вызывает в том же потоке, beginionvoke в новом
            Invoke(new ChatClient.Form1.ReciveFileRequestDelegate(ReciveFileRequest), e);

        }

        private void SendText_Click(object sender, EventArgs e)
        {
            _comPort.SendTextMessage(richTextBox2.Text, Convert.ToByte(textBox1.Text));
            richTextBox2.Clear();   
        }

        private void SendFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog();

            if (od.ShowDialog() == DialogResult.OK)
            {
                _comPort.SendFileTransferRequest(od.FileName, Convert.ToByte(textBox1.Text));
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _comPort.CancelSendingFile();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _comPort.CancelRecivingFile();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_comPort != null)
            {
                _comPort.Dispose();
            }
        }




    }
}
