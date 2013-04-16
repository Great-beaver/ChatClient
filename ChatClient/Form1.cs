﻿using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;
using Chat.Main;
using ChatClient.Main;


namespace ChatClient
{

    public partial class Form1 : Form
    {
        private ClientPort _comPort;

        public Form1()
        {
            InitializeComponent();
        }

        TabPage[] _tabPages = new TabPage[5];
        RichTextBox[] _richTextBoxs = new RichTextBox[5];

        private void Form1_Load(object sender, EventArgs e)
        {
         //  _comPort = new ClientPort(Properties.Settings.Default.ReadComPort, Properties.Settings.Default.WriteComPort, 
         //      Properties.Settings.Default.ClientId, Properties.Settings.Default.ComPortSpeed);
         //  // Подписывание на событие
         //  _comPort.MessageRecived += new EventHandler<MessageRecivedEventArgs>(ComPortMessageRecived);
         //  _comPort.FileRequestRecived += new EventHandler<FileRequestRecivedEventArgs>(ComPortFileRequestRecived);             
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
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Left;
                        _richTextBoxs[sender].SelectionColor = Color.Red;
                        _richTextBoxs[sender].AppendText("Клиент " + sender + " :" + '\n' + text + '\n');
                    }
                    break;

                case MessageType.TextDelivered:
                    {
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Right;
                        _richTextBoxs[sender].SelectionColor = Color.Green;
                        _richTextBoxs[sender].AppendText(text + '\n' +
                    "Сообщение для клиента " + sender + " доставлено." + '\n');
                    }
                    break;

                case MessageType.TextUndelivered:
                    {
                        _richTextBoxs[sender].AppendText(text + '\n' +
                    "Сообщение для клиента " + sender + "НЕ доставлено." + '\n');
                    }
                    break;

                case MessageType.FileUndelivered:
                    {
                        _richTextBoxs[sender].AppendText(
                    "Получатель " + sender + "не доступен, отправка файла отменена." + '\n');
                        CanBut.Visible = false;
                        progressBar1.Value = 0;
                        progressBar1.Visible = false;
                        timer1.Enabled = false;
                        FileBut.Visible = true;
                    }
                    break;

                case MessageType.MessageUndelivered:
                    {
                        _richTextBoxs[sender].AppendText("Клиента " + sender + "не доступен." + '\n');
                    }
                    break;

                case MessageType.FileReceivingComplete:
                    {
                        _richTextBoxs[sender].AppendText(
                        "Файл " + text + " от клиента " + sender + " получен." + '\n');
                        progressBar1.Visible = false;
                        progressBar1.Value = 0;
                        timer1.Enabled = false;
                        CanBut.Visible = false;
                        FileBut.Visible = true;

                        // Сохраняет содержимое буффера
                        IDataObject tmp = Clipboard.GetDataObject();


                        // Разрешить добавление в richtextbox элементов из буфера
                        _richTextBoxs[sender].ReadOnly = false; 

                        // Проверка является ли файл изображением
                        string e = Path.GetExtension(_comPort.ReceivingFileFullName);

                        if (e == ".jpg" || e == ".bmp")
                        {
                            // Вывод изображения
                            Image img = ResizeImg(Image.FromFile(_comPort.ReceivingFileFullName), 80, 60);
                             
                            Clipboard.SetImage(img);

                            _richTextBoxs[sender].Paste();
                        }

                        // Вставляет файл в richtextv
                        StringCollection paths = new StringCollection();
                        paths.Add(_comPort.ReceivingFileFullName);
                        Clipboard.SetFileDropList(paths);
                        _richTextBoxs[sender].Paste();

                        // Запретить вставку элементов
                        _richTextBoxs[sender].ReadOnly = true;

                        // Возвращает содерживмое буффера
                        if (tmp != null)
                        Clipboard.SetDataObject(tmp); 

                        // Добавляет пустую строку
                        _richTextBoxs[sender].AppendText(""+'\n');
                    }
                    break;

                case MessageType.FileSendingComplete:
                    {
                        _richTextBoxs[sender].AppendText(
                    "Передача файла " + text + " клиенту " + sender + " завершена." + '\n');
                        progressBar1.Visible = false;
                        CanBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                case MessageType.FileTransferAllowed:
                    {
                        _richTextBoxs[sender].AppendText(
                    "Клиент " + sender + " одобрил получение файла " + text +"." + '\n');
                        progressBar1.Visible = true;
                        progressBar1.Value = 0;
                        timer1.Enabled = true;
                        CanBut.Visible = true;
                    }
                    break;

                    case MessageType.FileTransferDenied:
                    {
                        _richTextBoxs[sender].AppendText(
                    "Клиент " + sender + " отклонил получение файла " + text +"." + '\n');
                        progressBar1.Visible = false;
                        progressBar1.Value = 0;
                        timer1.Enabled = false;
                        CanBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                    case MessageType.FileReceivingStarted:
                    {
                        _richTextBoxs[sender].AppendText(
                    "Начат прием файла  " + text + " от клиента " + sender + "." + '\n');
                        progressBar1.Visible = true;
                        progressBar1.Value = 0;
                        timer1.Enabled = true;
                        CanBut.Visible = true;
                        FileBut.Visible = false;
                    }
                    break;

                    case MessageType.FileTransferCanceled:
                    {
                        _richTextBoxs[sender].AppendText(
                    "Прием файла  " + text + " от клиента " + sender + " отменен." + '\n');
                        // Функция скрытия элементов приема файла
                        AllowBut.Visible = false;
                        DenyBut.Visible = false;
                        CanBut.Visible = false;
                        progressBar1.Visible = false;
                        progressBar1.Value = 0;
                        timer1.Enabled = false;
                        FileBut.Visible = true;

                    }
                    break;
                    case MessageType.FileTransferCanceledBySender:
                    {
                        _richTextBoxs[sender].AppendText(
                    "Передача файла " + text + " отменан отправителем " + sender +"." + '\n');
                        progressBar1.Visible = false;
                        progressBar1.Value = 0;
                        timer1.Enabled = false;
                        CanBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                    case MessageType.FileTransferCanceledByRecipient:
                    {
                        _richTextBoxs[sender].AppendText(
                    "Передача файла " + text + " отменан получателем " + sender +"." + '\n');
                        progressBar1.Value = 0;
                        progressBar1.Visible = false;
                        timer1.Enabled = false;
                        CanBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                    case MessageType.FileReceivingTimeOut:
                    {
                        _richTextBoxs[sender].AppendText(
                    "Вышло время ожидания файла " + text + " от " + sender + " передача отменена." + '\n');
                        progressBar1.Visible = false;
                        progressBar1.Value = 0;
                        timer1.Enabled = false;
                        CanBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                    case MessageType.FileRequestCanceledRecipientSide:
                    {
                        _richTextBoxs[sender].AppendText(
                            "Клиент " + sender + " отменил запрос на передачу файла."+ '\n');

                        AllowBut.Visible = false;
                        DenyBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                    case MessageType.FileRequestCanceledSenderSide:
                    {
                        _richTextBoxs[sender].AppendText(
                            "Запрос на передачу файла " + text + " отменен." + '\n');

                        AllowBut.Visible = false;
                        DenyBut.Visible = false;
                        CanBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                    case MessageType.ReadPortAvailable:
                    {
                        StatusLabelReadPortValue.Text = "Online";
                        StatusLabelReadPortValue.ForeColor = Color.Green;
                    }
                    break;

                    case MessageType.ReadPortUnavailable:
                    {
                        StatusLabelReadPortValue.Text = "Offline";
                        StatusLabelReadPortValue.ForeColor = Color.Red;
                    }
                    break;

                    case MessageType.WritePortAvailable:
                    {
                        StatusLabelWritePortValue.Text = "Online";
                        StatusLabelWritePortValue.ForeColor = Color.Green;
                    }
                    break;

                    case MessageType.WritePortUnavailable:
                    {
                        StatusLabelWritePortValue.Text = "Offline";
                        StatusLabelWritePortValue.ForeColor = Color.Red;
                    }
                    break;

                    case MessageType.WaitFileRecipientAnswer:
                    {
                        _richTextBoxs[sender].AppendText("Ожидается ответ клиента " + sender + " о передачи файла " + text + "." + '\n');
                    }
                    break;

                    case MessageType.Error:
                    {
                        _richTextBoxs[sender].AppendText(text + '\n');
                    }
                    break;
            }

          try
          {
             _richTextBoxs[sender].ScrollToCaret();
          }
          catch (Exception)
          {
              
          }
          
        }

        // Получение данных и вызов обработчика для текстовых сообщений
        void ComPortMessageRecived(object sender, MessageRecivedEventArgs e)
        {
            try
            {
                BeginInvoke(new ReciveMessageDelegate(ReciveMessage), e.MessageType, e.MessageText, e.Sender);
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
            
           // // Сообщения о том надо ли принимать файл
           // DialogResult dialogResult = MessageBox.Show("Прниять файл " +e.FileName  +
           //        "от клиента № " + e.Sender + " размером " + e.FileLenght / 1024 / 1024 + "МБ?",
           //        "Передача файла", MessageBoxButtons.YesNo);
           //
           // e.FileTransferAllowed = dialogResult == DialogResult.Yes;

            _richTextBoxs[e.Sender].AppendText("Прниять файл " + e.FileName +
                   "от клиента № " + e.Sender + " размером " + e.FileLenght / 1024 / 1024 + "МБ?" + '\n');

            // Скрол вниз
            if (_richTextBoxs[e.Sender].TextLength > 0)
                _richTextBoxs[e.Sender].Select(_richTextBoxs[e.Sender].TextLength - 1, 0);
            _richTextBoxs[e.Sender].ScrollToCaret();

            FileBut.Visible = false;

            AllowBut.Visible = true;
            DenyBut.Visible = true;

        }

        // Получение данных и вызов обработчика запроса на передачу
        void ComPortFileRequestRecived(object sender, FileRequestRecivedEventArgs e)
        {
            // ivoke вызывает в том же потоке, beginionvoke в новом
            BeginInvoke(new ReciveFileRequestDelegate(ReciveFileRequest), e);

        }

        private void button1_Click(object sender, EventArgs e)
        {  
          //MessageBox.Show(tabControl1.SelectedTab.ToString());
            _comPort.SendTextMessage(writeRichTextBox.Text, (byte)tabControl1.SelectedTab.TabIndex);
            writeRichTextBox.Clear();                
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _comPort = new ClientPort(textBox1.Text, textBox2.Text,
                 Convert.ToByte(textBox3.Text), Convert.ToInt32(textBox5.Text));
             // Подписывание на событие

            StatusLabelIDValue.Text = _comPort.ClietnId.ToString();

             _comPort.MessageRecived += new EventHandler<MessageRecivedEventArgs>(ComPortMessageRecived);
             _comPort.FileRequestRecived += new EventHandler<FileRequestRecivedEventArgs>(ComPortFileRequestRecived);

            for (int i = 0; i < 5; i++)
            {
                _richTextBoxs[i] = new RichTextBox();
                if (i!=_comPort.ClietnId)
                {
                    //string title = "TabPage " + (tabControl1.TabCount + 1).ToString();
                    _tabPages[i] = new TabPage("Клиент " + i);
                    _tabPages[i].TabIndex = i;
                    
                    _richTextBoxs[i].Name = "ClientRichTextBox" + i;
                    _richTextBoxs[i].Left = 1;
                    _richTextBoxs[i].Top = 1;
                    _richTextBoxs[i].Width = tabControl1.Width - 10;
                    _richTextBoxs[i].Height = tabControl1.Height -28;
                    _richTextBoxs[i].BackColor = Color.Beige;

                    //_richTextBoxs[i].AppendText(_tabPages[i].TabIndex.ToString());

                    _richTextBoxs[i].ReadOnly = true;

                    _tabPages[i].Controls.Add(_richTextBoxs[i]);

                    tabControl1.TabPages.Add(_tabPages[i]);
                }
            }       
        }



        private void timer1_Tick(object sender, EventArgs e)
        {

            float receivedPacketPercentage;

            if (_comPort.IsRecivingFile)
            {
                receivedPacketPercentage = (float)_comPort.ProcessedFileSize / _comPort.ReceivingFileSize * 100;
            }
            else
            {
                receivedPacketPercentage = (float)_comPort.ProcessedFileSize / _comPort.FileToTransfer.Length * 100;
            }

            if (receivedPacketPercentage < 100)
            {
                progressBar1.Value = (int)receivedPacketPercentage;
            }
            else
            {
                progressBar1.Value = 100;
            }

        }




        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_comPort != null)
            {
                _comPort.Dispose();
            }
        }

        private void AllowBut_Click(object sender, EventArgs e)
        {
            AllowBut.Visible = false;
            DenyBut.Visible = false;
            progressBar1.Visible = true;
            //CanBut.Visible = true;
            _comPort.AllowFileTransfer();         
        }

        private void DenyBut_Click(object sender, EventArgs e)
        {
            AllowBut.Visible = false;
            DenyBut.Visible = false;
            _comPort.DenyFileTransfer();
        }



        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }



        public Image ResizeImg(Image b, int nWidth, int nHeight)
        {
            Image result = new Bitmap(nWidth, nHeight);
            using (Graphics g = Graphics.FromImage((Image)result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(b, 0, 0, nWidth, nHeight);
                g.Dispose();
            }
            return result;
        }



        private void CanBut_Click(object sender, EventArgs e)
        {
            _comPort.CancelSendingFile();
            _comPort.CancelRecivingFile();
            _comPort.CancleFileRequest();
            progressBar1.Value = 0;
            timer1.Enabled = false;
            CanBut.Visible = false;
            FileBut.Visible = true;
        }

        private void FileBut_Click(object sender, EventArgs e)
        { 
            OpenFileDialog od = new OpenFileDialog();

            if (od.ShowDialog() == DialogResult.OK)
            {
                _comPort.SendFileTransferRequest(od.FileName, (byte)tabControl1.SelectedTab.TabIndex);
            }

            CanBut.Visible = true;
            FileBut.Visible = false;

        }

    }
}
