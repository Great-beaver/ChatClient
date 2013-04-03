using System;
using System.Text;
using System.Windows.Forms;
using ChatClient.Main;

namespace ChatClient
{

    public partial class Form1 : Form
    {
        private ClientPort comPort;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            

        }

        // Делегат обработчика  текстовых сообщений 
        public delegate void ReciveMessageDelegate(string type, string text, byte sender);

        // Метод обработки текстовых сообщений 
        void ReciveMessage(string type, string text, byte sender)
        {
            switch (type)
            {
                case "Text" :
                    {
                        richTextBox1.AppendText("Клиент " + sender + " :"+ '\n' + text + '\n');
                    }
                    break;

                    case "ACK":
                    {
                        richTextBox1.AppendText(text + '\n'+
                    "Сообщение для клиента " + sender + " доставлено." + '\n');
                    }
                    break;

                    case "TextUndelivered":
                    {
                        richTextBox1.AppendText(text + '\n' +
                    "Сообщение для клиента " + sender + "НЕ доставлено." + '\n');
                    }
                    break;

                    case "FileUndelivered":
                    {
                        richTextBox1.AppendText(
                    "Получатель " + sender + "не доступен, отправка файла отменена." + '\n');
                    }
                    break;

                    case "MessageUndelivered":
                    {
                        richTextBox1.AppendText("Клиента " + sender + "не лоступен." + '\n');
                    }
                    break;

                    case "FileReceivingComplete":
                    {
                        richTextBox1.AppendText(
                    "Файл " + text + " от клиента " + sender + " получен." + '\n');
                    }
                    break;

                    case "FileSendingComplete":
                    {
                        richTextBox1.AppendText(
                    "Передача файла " + text + " клиенту " + sender + " завершена." + '\n');
                    }
                    break;

                    case "FileTransferAllowed":
                    {
                        richTextBox1.AppendText(
                    "Клиент " + sender + " одобрил получение файла " + text +"." + '\n');
                    }
                    break;

                    case "FileTransferDenied":
                    {
                        richTextBox1.AppendText(
                    "Клиент " + sender + " отклонил получение файла " + text +"." + '\n');
                    }
                    break;

                    case "FileReceivingStarted":
                    {
                    richTextBox1.AppendText(
                    "Начат прием файла  " + text + " от клиента " + sender + "." + '\n');
                    }
                    break;

                    case "FileTransferCanceled":
                    {
                    richTextBox1.AppendText(
                    "Прием файла  " + text + " от клиента " + sender + " отменен." + '\n');
                    }
                    break;
                    case "FileTransferCanceledBySender":
                    {
                        richTextBox1.AppendText(
                    "Передача файла " + text + " отменан отправителем " + sender +"." + '\n');
                    }
                    break;

                    case "FileTransferCanceledByRecipient":
                    {
                        richTextBox1.AppendText(
                    "Передача файла " + text + " отменан получателем " + sender +"." + '\n');
                    }
                    break;

                    case "FileReceivingTimeOut":
                    {
                        richTextBox1.AppendText(
                    "Вышло время ожидания файла " + text + " от " + sender + " передача отменена." + '\n');
                    }
                    break;

            }

            // Скрол вниз
            if (richTextBox1.TextLength>0)
            richTextBox1.Select(this.richTextBox1.TextLength - 1, 0);
            richTextBox1.ScrollToCaret();

        }

        // Получение данных и вызов обработчика для текстовых сообщений
        void ComPortMessageRecived(object sender, MessageRecivedEventArgs e)
        {
            BeginInvoke(new ReciveMessageDelegate(ReciveMessage), e.MessageType, e.MessageText, e.Sender);
        }

        // Делегат обработчика запроса на передачу
        public delegate void ReciveFileRequestDelegate(FileRequestRecivedEventArgs e);

        // Метод обработки запроса на передачу
        void ReciveFileRequest(FileRequestRecivedEventArgs e)
        {
          //  MessageBox.Show("Запрос на передачу файла " + fileName + " размером " + fileLenght / 1024 / 1024 + "МБ" + " от клиента " + sender);
            
            // Сообщения о том надо ли принимать файл
            DialogResult dialogResult = MessageBox.Show("Прниять файл " +e.FileName  +
                   "от клиента № " + e.Sender + " размером " + e.FileLenght / 1024 / 1024 + "МБ?",
                   "Передача файла", MessageBoxButtons.YesNo);

            e.FileTransferAllowed = dialogResult == DialogResult.Yes;
        }

        // Получение данных и вызов обработчика запроса на передачу
        void ComPortFileRequestRecived(object sender, FileRequestRecivedEventArgs e)
        {
            // ivoke вызывает в том же потоке, beginionvoke в новом
            Invoke(new ReciveFileRequestDelegate(ReciveFileRequest), e);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            comPort.SendTextMessage(richTextBox2.Text, Convert.ToByte(textBox4.Text));
            richTextBox2.Clear();    
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            comPort = new ClientPort(textBox1.Text, textBox2.Text, Convert.ToByte(textBox3.Text),Convert.ToInt32(textBox5.Text));
            // Подписывание на событие
            comPort.MessageRecived += new EventHandler<MessageRecivedEventArgs>(ComPortMessageRecived);
            comPort.FileRequestRecived += new EventHandler<FileRequestRecivedEventArgs>(ComPortFileRequestRecived);
          
            timer1.Enabled = true;            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog();

            if (od.ShowDialog() == DialogResult.OK)
            {
                comPort.SendFileTransferRequest(od.FileName, Convert.ToByte(textBox4.Text));
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lock (comPort.InputMessageQueue)
            {
                if (comPort.InputMessageQueue.Count > 0)
                {
                    richTextBox1.AppendText(comPort.InputMessageQueue.Dequeue().ToString() + '\n');
                }
            }           
        }

        private void button4_Click(object sender, EventArgs e)
        {
            comPort.CancelSendingFile();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            comPort.CancelRecivingFile();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (comPort != null)
            {
                comPort.Dispose();
            }
        }
    }
}
