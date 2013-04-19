using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Chat.Main;
using ChatClient.Main;

namespace Server
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
        private ToolStripStatusLabel[] _readPortsStatusLabels = new ToolStripStatusLabel[4];
        private ToolStripStatusLabel[] _readPortsStatusLabelsValue = new ToolStripStatusLabel[4];
        private ToolStripStatusLabel _writePortStatusLabel = new ToolStripStatusLabel("Write port");
        private ToolStripStatusLabel _writePortStatusLabelValue = new ToolStripStatusLabel("Write port Value");
        private ToolStripStatusLabel _iDtatusLabel = new ToolStripStatusLabel("ID");
        private ToolStripStatusLabel _iDtatusLabelValue = new ToolStripStatusLabel("ID Value");

        int [] _newMessageCount = new int[5];
       
        private void Form1_Load(object sender, EventArgs e)
        {
            _comPort = new ClientPort(Properties.Settings.Default.readerPortName1, Properties.Settings.Default.readerPortName2, Properties.Settings.Default.readerPortName3,
              Properties.Settings.Default.readerPortName4, Properties.Settings.Default.WriteComPort, Properties.Settings.Default.ClientId, Properties.Settings.Default.ComPortSpeed);
            // Подписывание на событие
            _comPort.MessageRecived += new EventHandler<MessageRecivedEventArgs>(ComPortMessageRecived);
            _comPort.FileRequestRecived += new EventHandler<FileRequestRecivedEventArgs>(ComPortFileRequestRecived);  

            for (int i = 0; i < _newMessageCount.Length; i++)
            {
                _newMessageCount[i] = 0;
            }

            for (int i = 0; i < _readPortsStatusLabels.Length; i++)
            {
                _readPortsStatusLabels[i] = new ToolStripStatusLabel("Read Port " + (i+1));
                statusStrip1.Items.Add(_readPortsStatusLabels[i]);

                _readPortsStatusLabelsValue[i] = new ToolStripStatusLabel("Read Port " + (i + 1)+"Value");
               // _readPortsStatusLabelsValue[i].Text = "";
                statusStrip1.Items.Add(_readPortsStatusLabelsValue[i]);
            }

            statusStrip1.Items.Add(_writePortStatusLabel);
            statusStrip1.Items.Add(_writePortStatusLabelValue);

            statusStrip1.Items.Add(_iDtatusLabel);
            statusStrip1.Items.Add(_iDtatusLabelValue);

         //  _comPort = new ClientPort(Properties.Settings.Default.ReadComPort, Properties.Settings.Default.WriteComPort, 
         //      Properties.Settings.Default.ClientId, Properties.Settings.Default.ComPortSpeed);
         //  // Подписывание на событие
         //  _comPort.MessageRecived += new EventHandler<MessageRecivedEventArgs>(ComPortMessageRecived);
         //  _comPort.FileRequestRecived += new EventHandler<FileRequestRecivedEventArgs>(ComPortFileRequestRecived);      
  
            writeRichTextBox.AllowDrop = true;
            writeRichTextBox.DragDrop += new DragEventHandler(writeRichTextBox_DragDrop);
            writeRichTextBox.DragEnter += new DragEventHandler(writeRichTextBox_DragEnter);
            writeRichTextBox.DragOver += new DragEventHandler(writeRichTextBox_DragOver);

            // Подписывание на событие

            _iDtatusLabelValue.Text = _comPort.ClietnId.ToString();

            for (int i = 0; i < 5; i++)
            {
                _richTextBoxs[i] = new RichTextBox();
                if (i != _comPort.ClietnId)
                {
                    //string title = "TabPage " + (tabControl1.TabCount + 1).ToString();
                    _tabPages[i] = new TabPage("Клиент " + i);
                    _tabPages[i].TabIndex = i;

                    _richTextBoxs[i].Name = "ClientRichTextBox" + i;
                    _richTextBoxs[i].Left = 1;
                    _richTextBoxs[i].Top = 1;
                    _richTextBoxs[i].Width = tabControl1.Width - 10;
                    _richTextBoxs[i].Height = tabControl1.Height - 28;
                    _richTextBoxs[i].BackColor = Color.Beige;
                    //  _richTextBoxs[i].Font = new Font("Microsoft Sans Serif", 10);

                    //_richTextBoxs[i].AppendText(_tabPages[i].TabIndex.ToString());

                    _richTextBoxs[i].ReadOnly = true;

                    _tabPages[i].Controls.Add(_richTextBoxs[i]);

                    tabControl1.TabPages.Add(_tabPages[i]);
                }
            }  

        }

        private void writeRichTextBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && FileBut.Visible)
            {
                string[] docPath = (string[]) e.Data.GetData(DataFormats.FileDrop);

                _comPort.SendFileTransferRequest(docPath[0], (byte)tabControl1.SelectedTab.TabIndex);
                CanBut.Visible = true;
                FileBut.Visible = false;
            }
            writeRichTextBox.Clear(); 
        }

        private void writeRichTextBox_DragEnter(object sender, DragEventArgs e)
        {
                e.Effect = DragDropEffects.Copy;
        }

        private void writeRichTextBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        // Делегат обработчика  текстовых сообщений 
        public delegate void ReciveMessageDelegate(MessageType type, string text, byte sender, byte recipient);

        // Метод обработки текстовых сообщений 
        void ReciveMessage(MessageType type, string text, byte sender,byte recipient) 
        {
            switch (type)
            {
                case MessageType.Text:
                    {
                        if (recipient != _comPort.ClietnId)
                        {
                            _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Center;
                            _richTextBoxs[sender].SelectionFont = new Font("Microsoft Sans Serif", 7);
                            _richTextBoxs[sender].SelectionColor = Color.SteelBlue;
                            _richTextBoxs[sender].AppendText("Отправлено клиенту " + recipient + '\n');

                            _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Center;
                            _richTextBoxs[sender].SelectionFont = new Font("Microsoft Sans Serif", 8);
                            _richTextBoxs[sender].SelectionColor = Color.SteelBlue;
                            _richTextBoxs[sender].AppendText(text + '\n');

                        }
                        else
                        {
                            _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Left;
                            _richTextBoxs[sender].SelectionColor = Color.Black;
                            _richTextBoxs[sender].AppendText(text + '\n');  
                        }

                        

                        if (tabControl1.SelectedTab.TabIndex != sender)
                        {
                            _tabPages[sender].Text = "Клиент " + sender + " +" + ++_newMessageCount[sender] ;                                
                        }
                    }
                    break;

                case MessageType.TextDelivered:
                    {
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Right;
                        _richTextBoxs[sender].SelectionColor = Color.Black;
                        _richTextBoxs[sender].AppendText(text + '\n');

                        _richTextBoxs[sender].SelectionColor = Color.Green;
                        _richTextBoxs[sender].SelectionFont = new Font("Microsoft Sans Serif", 7);
                        _richTextBoxs[sender].AppendText("Доставлено." + '\n');
                    }
                    break;

                case MessageType.TextUndelivered:
                    {
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Right;
                        _richTextBoxs[sender].SelectionColor = Color.Black;
                        _richTextBoxs[sender].AppendText(text + '\n');

                        _richTextBoxs[sender].SelectionColor = Color.Red;
                        _richTextBoxs[sender].SelectionFont = new Font("Microsoft Sans Serif", 7);
                        _richTextBoxs[sender].AppendText("Не доставлено." + '\n');
                    }
                    break;

                case MessageType.Resend:
                    {

                   //   // byte attempts;
                   //   // byte.TryParse(text, out attempts);
                   //   //
                   //   // 
                   //   // if (attempts == 1)
                   //   // {
                   //   //     _richTextBoxs[sender].AppendText(text + '\n');   
                   //   // }
                   //
                   //    
                   //
                   //    _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Right;
                   //
                   //    _richTextBoxs[sender].SelectionColor = Color.SteelBlue;
                   //    _richTextBoxs[sender].SelectionFont = new Font("Microsoft Sans Serif", 10);
                   //    _richTextBoxs[sender].AppendText('\u25CF' + "");

                    }
                    break;


                case MessageType.FileUndelivered:
                    {
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Left;
                        _richTextBoxs[sender].SelectionColor = Color.DarkRed;
                        _richTextBoxs[sender].AppendText(
                    "Получатель " + sender + " не доступен, отправка файла отменена." + '\n');
                        CanBut.Visible = false;
                        FileLabel.Visible = false;
                        progressBar1.Value = 0;
                        progressBar1.Visible = false;
                        timer1.Enabled = false;
                        FileBut.Visible = true;
                    }
                    break;

                case MessageType.MessageUndelivered:
                    {
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Left;
                        _richTextBoxs[sender].SelectionColor = Color.DarkRed;
                        _richTextBoxs[sender].AppendText("Клиента " + sender + " не доступен." + '\n');
                        CanBut.Visible = false;
                        FileLabel.Visible = false;
                        progressBar1.Value = 0;
                        progressBar1.Visible = false;
                        timer1.Enabled = false;
                        FileBut.Visible = true;
                    }
                    break;

                case MessageType.FileReceivingComplete:
                    {
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Left;
                        _richTextBoxs[sender].SelectionColor = Color.DarkGreen;
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

                        try
                        {
                            // Возвращает содерживмое буффера
                            if (tmp != null)
                                Clipboard.SetDataObject(tmp); 
                        }
                        catch (Exception)
                        {
                            
                        }


                        

                        // Добавляет пустую строку
                        _richTextBoxs[sender].AppendText(""+'\n');
                    }
                    break;

                case MessageType.FileSendingComplete:
                    {
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Left;
                        _richTextBoxs[sender].SelectionColor = Color.DarkGreen;
                        _richTextBoxs[sender].AppendText("Передача файла " + text + " клиенту " + sender + " завершена." + '\n');
                        progressBar1.Visible = false;
                        CanBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                case MessageType.FileTransferAllowed:
                    {
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Left;
                        _richTextBoxs[sender].SelectionColor = Color.DarkGreen;
                        _richTextBoxs[sender].AppendText(
                    "Клиент одобрил получение файла." + '\n');
                        progressBar1.Visible = true;
                        progressBar1.Value = 0;
                        timer1.Enabled = true;
                        CanBut.Visible = true;
                    }
                    break;

                    case MessageType.FileTransferDenied:
                    {
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Left;
                        _richTextBoxs[sender].SelectionColor = Color.DarkRed;
                        _richTextBoxs[sender].AppendText(
                    "Клиент отклонил получение файла." + '\n');
                        progressBar1.Visible = false;
                        progressBar1.Value = 0;
                        timer1.Enabled = false;
                        CanBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                    case MessageType.FileReceivingStarted:
                    {
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Left;
                        _richTextBoxs[sender].SelectionColor = Color.DarkGreen;
                        _richTextBoxs[sender].AppendText(
                    "Начат прием файла  " + text + "." + '\n');
                        progressBar1.Visible = true;
                        progressBar1.Value = 0;
                        timer1.Enabled = true;
                        CanBut.Visible = true;
                        FileBut.Visible = false;
                    }
                    break;

                    case MessageType.FileTransferCanceledRecipientSide:
                    {
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Left;
                        _richTextBoxs[sender].SelectionColor = Color.DarkRed;
                        _richTextBoxs[sender].AppendText(
                    "Прием файла отменен." + '\n');
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

                    case MessageType.FileTransferCanceledSenderSide:
                    {
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Left;
                        _richTextBoxs[sender].SelectionColor = Color.DarkRed;
                        _richTextBoxs[sender].AppendText(
                    "Передачай файла отменена." + '\n');
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
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Left;
                        _richTextBoxs[sender].SelectionColor = Color.DarkRed;
                        _richTextBoxs[sender].AppendText(
                    "Передача файла отменена отправителем." + '\n');
                        progressBar1.Visible = false;
                        progressBar1.Value = 0;
                        timer1.Enabled = false;
                        CanBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                    case MessageType.FileTransferCanceledByRecipient:
                    {
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Left;
                        _richTextBoxs[sender].SelectionColor = Color.DarkRed;
                        _richTextBoxs[sender].AppendText(
                    "Передача файла отменена получателем." + '\n');
                        progressBar1.Value = 0;
                        progressBar1.Visible = false;
                        timer1.Enabled = false;
                        CanBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                    case MessageType.FileReceivingTimeOut:
                    {
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Left;
                        _richTextBoxs[sender].SelectionColor = Color.DarkRed;
                        _richTextBoxs[sender].AppendText(
                    "Вышло время ожидания файла " + text + " передача отменена." + '\n');
                        progressBar1.Visible = false;
                        progressBar1.Value = 0;
                        timer1.Enabled = false;
                        CanBut.Visible = false;
                        FileBut.Visible = true;
                        FileLabel.Visible = false;
                        AllowBut.Visible = false;
                        DenyBut.Visible = false;
                    }
                    break;

                    case MessageType.FileRequestCanceledRecipientSide:
                    {
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Left;
                        _richTextBoxs[sender].SelectionColor = Color.DarkRed;
                        _richTextBoxs[sender].AppendText(
                            "Клиент отменил запрос на передачу файла."+ '\n');

                        FileLabel.Visible = false;
                        AllowBut.Visible = false;
                        DenyBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                    case MessageType.FileRequestCanceledSenderSide:
                    {
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Left;
                        _richTextBoxs[sender].SelectionColor = Color.DarkRed;
                        _richTextBoxs[sender].AppendText(
                            "Запрос на передачу файла отменен." + '\n');

                        FileLabel.Visible = false;
                        AllowBut.Visible = false;
                        DenyBut.Visible = false;
                        CanBut.Visible = false;
                        FileBut.Visible = true;
                        progressBar1.Visible = false;
                    }
                    break;

                    case MessageType.ReadPortAvailable:
                    {
                        _readPortsStatusLabelsValue[sender].Text = "Online";
                        _readPortsStatusLabelsValue[sender].ForeColor = Color.Green;
                        return;
                    }
                    break;

                    case MessageType.ReadPortUnavailable:
                    {
                        _readPortsStatusLabelsValue[sender].Text = "Offline";
                        _readPortsStatusLabelsValue[sender].ForeColor = Color.Red;
                        return;
                    }
                    break;

                    case MessageType.WritePortAvailable:
                    {
                        _writePortStatusLabelValue.Text = "Online";
                        _writePortStatusLabelValue.ForeColor = Color.Green;
                        return;
                    }
                    break;

                    case MessageType.WritePortUnavailable:
                    {
                        _writePortStatusLabelValue.Text = "Offline";
                        _writePortStatusLabelValue.ForeColor = Color.Red;
                        return;
                    }
                    break;

                    case MessageType.WaitFileRecipientAnswer:
                    {
                        _richTextBoxs[sender].SelectionAlignment = HorizontalAlignment.Left;
                        _richTextBoxs[sender].SelectionColor = Color.SteelBlue;
                        _richTextBoxs[sender].AppendText("Ожидается ответ клиента о передачи файла " + text + "." + '\n');
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
                BeginInvoke(new ReciveMessageDelegate(ReciveMessage), e.MessageType, e.MessageText, e.Sender, e.Recipient);
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

            float filesize = (float)e.FileLenght / 1024 / 1024;

            _richTextBoxs[e.Sender].SelectionAlignment = HorizontalAlignment.Left;
            _richTextBoxs[e.Sender].SelectionColor = Color.SteelBlue;
            _richTextBoxs[e.Sender].AppendText("Прниять файл " + e.FileName + " размером " + filesize.ToString("0.00") + "МБ?" + '\n');

            // Скрол вниз
            if (_richTextBoxs[e.Sender].TextLength > 0)
                _richTextBoxs[e.Sender].Select(_richTextBoxs[e.Sender].TextLength - 1, 0);
            _richTextBoxs[e.Sender].ScrollToCaret();

            FileBut.Visible = false;

            AllowBut.Visible = true;
            DenyBut.Visible = true;
            FileLabel.Visible = true;

            if (tabControl1.SelectedTab.TabIndex != e.Sender)
            {
                _tabPages[e.Sender].Text = "Клиент " + e.Sender + " +" + ++_newMessageCount[e.Sender];
            }


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
            if (_comPort.SendTextMessage(writeRichTextBox.Text, (byte)tabControl1.SelectedTab.TabIndex))
            {
                writeRichTextBox.Clear();    
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
            FileLabel.Visible = false;
            progressBar1.Visible = true;
            //CanBut.Visible = true;
            _comPort.AllowFileTransfer();         
        }

        private void DenyBut_Click(object sender, EventArgs e)
        {
            AllowBut.Visible = false;
            DenyBut.Visible = false;
            FileLabel.Visible = false;
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
                CanBut.Visible = true;
                FileBut.Visible = false;
            }
        }

        private void writeRichTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void writeRichTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                if (_comPort.SendTextMessage(writeRichTextBox.Text, (byte)tabControl1.SelectedTab.TabIndex))
                {
                    writeRichTextBox.Clear();    
                }  
            }
        }

        private void writeRichTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void writeRichTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && writeRichTextBox.Lines[0] == "")
            {
                writeRichTextBox.Clear();
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
           // MessageBox.Show(tabControl1.SelectedTab.TabIndex.ToString());

            tabControl1.SelectedTab.Text = "Клиент " + tabControl1.SelectedTab.TabIndex;
            _newMessageCount[tabControl1.SelectedTab.TabIndex] = 0;

        }

        private void StatusLabelWritePort_Click(object sender, EventArgs e)
        {

        }
    }
}
