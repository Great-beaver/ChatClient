using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Chat.Main;
using ChatClient.Main;
using DevExpress.Office.PInvoke;
using DevExpress.XtraEditors;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using DevExpress.XtraTab;


namespace Client
{
    public partial class XtraForm1 : XtraForm
    {
        public XtraForm1()
        {
            InitializeComponent();
        }

        CommunicationUnit _cu;
        XtraTabPage[] _tabPages = new XtraTabPage[5];
        RichEditControl[] _richEditControls = new RichEditControl[5];

        Font _defaultFont = new Font("Tahoma",14);
        Font _deliveryFont = new Font("Microsoft Sans Serif", 9);

        Font _headerFont = new Font("Tahoma", 12,FontStyle.Bold);

        Font _buttonFont = new Font("Tahoma", 10, FontStyle.Bold);

        private Boolean _needClearWriteRichEditControl = false;
            
        Color _positiveColor = Color.Green;
        Color _negativeColor = Color.Red;
        Color _neutralColor = Color.Black;
        Color _systemColor = Color.SteelBlue;

        int[] _newMessageCount = new int[5];

        string[] _quickCommands = Properties.Settings.Default.QucikCommands.Split('|');


        private void QuickCommandsInitialize()
        {
            foreach (string command in _quickCommands)
            {
                var b = new SimpleButton();
                b.Text = command;
                b.Font = _buttonFont;
                //b.Size = new Size(0, 33);
                Size size = b.CalcBestSize();
                size.Width = Math.Max(size.Width, b.Width);
                // size.Height = Math.Max(size.Height, 33);
                size.Height = 43;
                b.Size = size;
                b.Click += (s, ea) =>
                {
                    if (s is SimpleButton)
                    {
                        var button = s as SimpleButton;
                        writeRichEditControl.Document.AppendText(button.Text + "\n");
                        writeRichEditControl.Focus();
                    }
                };
                flowLayoutPanel1.Controls.Add(b);
            }
        }

        private void RichEditControlsInitialize()
        {
            for (int i = 0; i < 5; i++)
            {
                _richEditControls[i] = new RichEditControl();
                if (i != _cu.ClietnId)
                {
                    //string title = "TabPage " + (tabControl1.TabCount + 1).ToString();
                    _tabPages[i] = new XtraTabPage();
                    _tabPages[i].Text = "Клиент " + i;
                    _tabPages[i].Tag = i;
                    _tabPages[i].Appearance.Header.Font = _headerFont;

                    _richEditControls[i].Name = "ClientRichEditControl" + i;
                    _richEditControls[i].Left = 1;
                    _richEditControls[i].Top = 1;
                    _richEditControls[i].Width = xtraTabControl1.Width - 5;
                    _richEditControls[i].Height = xtraTabControl1.Height - 28;
                    _richEditControls[i].Options.HorizontalRuler.Visibility = RichEditRulerVisibility.Hidden;
                    _richEditControls[i].Options.HorizontalScrollbar.Visibility = RichEditScrollbarVisibility.Hidden;
                    _richEditControls[i].Options.VerticalRuler.Visibility = RichEditRulerVisibility.Hidden;
                    _richEditControls[i].Options.VerticalScrollbar.Visibility = RichEditScrollbarVisibility.Visible;
                    _richEditControls[i].Options.Hyperlinks.ModifierKeys = Keys.None;
                    _richEditControls[i].ActiveViewType = RichEditViewType.Simple;
                    _richEditControls[i].Views.SimpleView.Padding = new Padding(5, 4, 4, 0);
                    _richEditControls[i].ReadOnly = true;
                    _richEditControls[i].ShowCaretInReadOnly = false;
                    _richEditControls[i].Dock = DockStyle.Fill;
                    _richEditControls[i].PopupMenuShowing += new PopupMenuShowingEventHandler(HideContextMenu);
                    _richEditControls[i].Click += (s, ea) =>
                    {
                        writeRichEditControl.Focus();
                    };

                    _tabPages[i].Controls.Add(_richEditControls[i]);

                    xtraTabControl1.TabPages.Add(_tabPages[i]);
                }
            }


            writeRichEditControl.AllowDrop = true;
            writeRichEditControl.DragDrop += new DragEventHandler(writeRichEditControl_DragDrop);
            writeRichEditControl.DragEnter += new DragEventHandler(writeRichEditControl_DragEnter);
            writeRichEditControl.DragOver += new DragEventHandler(writeRichEditControl_DragOver);
        }


        private void XtraForm1_Load(object sender, EventArgs e)
        {           

           MaximizedBounds = Screen.GetWorkingArea(this);
           WindowState = FormWindowState.Maximized;
           writeRichEditControl.Font = _defaultFont;
           ShowKeyboard();


            QuickCommandsInitialize();


           for (int i = 0; i < _newMessageCount.Length; i++)
           {
               _newMessageCount[i] = 0;
           }
          

            _cu = new CommunicationUnit(Properties.Settings.Default.ReadPort, Properties.Settings.Default.WritePort, 
                  Properties.Settings.Default.ClientId, Properties.Settings.Default.PortSpeed);
              // Подписывание на событие

            _cu.MessageRecived += new EventHandler<MessageRecivedEventArgs>(ComPortMessageRecived);
            _cu.FileRequestRecived += new EventHandler<FileRequestRecivedEventArgs>(ComPortFileRequestRecived);

            IdValueLabelControl.Text = _cu.ClietnId.ToString();


            RichEditControlsInitialize();


           
        }

        // Делегат обработчика  текстовых сообщений 
        public delegate void ReciveMessageDelegate(MessageType type, string text, byte sender, byte recipient);

        // Метод обработки текстовых сообщений 
        void ReciveMessage(MessageType type, string text, byte sender, byte recipient)
        {
            switch (type)
            {
                case MessageType.Text:
                    {
                        AppendLine(text, _richEditControls[sender], _defaultFont, _neutralColor, ParagraphAlignment.Left);

                        if (Convert.ToByte(xtraTabControl1.SelectedTabPage.Tag) != sender)
                        {
                            _tabPages[sender].Text = "Клиент " + sender + " +" + ++_newMessageCount[sender];
                            return;
                        }

                    }
                    break;

                case MessageType.TextDelivered:
                    {
                        AppendLine(text, _richEditControls[sender], _defaultFont, _neutralColor, ParagraphAlignment.Right);

                        AppendLine("Доставлено.", _richEditControls[sender], _deliveryFont, _positiveColor, ParagraphAlignment.Right);

                    }
                    break;

                case MessageType.TextUndelivered:
                    {
                        AppendLine(text, _richEditControls[sender], _defaultFont, _neutralColor, ParagraphAlignment.Right);

                        AppendLine("Не доставлено.", _richEditControls[sender], _deliveryFont, _negativeColor, ParagraphAlignment.Right);

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
                        AppendLine("Получатель " + sender + " не доступен, отправка файла отменена.", _richEditControls[sender], _defaultFont, _negativeColor, ParagraphAlignment.Left);

                        CancelBut.Visible = false;
                        FileRequestLabel.Visible = false;
                        progressBarControl1.EditValue = 0;
                        progressBarControl1.Visible = false;
                        timer1.Enabled = false;
                        FileBut.Visible = true;
                    }
                    break;

                case MessageType.MessageUndelivered:
                    {
                        AppendLine("Клиента " + sender + " не доступен.", _richEditControls[sender], _defaultFont, _negativeColor, ParagraphAlignment.Left);

                        CancelBut.Visible = false;
                        FileRequestLabel.Visible = false;
                        progressBarControl1.EditValue = 0;
                        progressBarControl1.Visible = false;
                        timer1.Enabled = false;
                        FileBut.Visible = true;
                    }
                    break;

                case MessageType.FileReceivingComplete:
                    {
                        AppendLine("Файл " + text + " от клиента " + sender + " получен.", _richEditControls[sender], _defaultFont, _positiveColor, ParagraphAlignment.Left);

                        progressBarControl1.Visible = false;
                        progressBarControl1.EditValue = 0;
                        timer1.Enabled = false;
                        CancelBut.Visible = false;
                        FileBut.Visible = true;

                        // Проверка является ли файл изображением
                        string e = Path.GetExtension(_cu.ReceivingFileFullName);

                        if (e == ".jpg" || e == ".bmp" || e == ".png" || e == ".jpeg")
                        {
                          // Ресайз картинки 
                         Image img = ResizeImg(Image.FromFile(_cu.ReceivingFileFullName), 320, 240);
                         
                          // Вставка картинки
                          _richEditControls[sender].Document.CaretPosition = _richEditControls[sender].Document.Range.End;
                          _richEditControls[sender].Document.InsertImage(_richEditControls[sender].Document.CaretPosition, img);
                          // Добавляет пустую строку для отступа
                          _richEditControls[sender].Document.AppendText("" + '\n');
                        }
                        
                        // Вставка ссылки на открытие файла
                        AddHyperLink("Открыть файл", _cu.ReceivingFileFullName, _richEditControls[sender],_defaultFont);

                        // Добавляет пустую строку для отступа
                        _richEditControls[sender].Document.AppendText(""+'\n');

                        

                    }
                    break;

                case MessageType.FileSendingComplete:
                    {
                        AppendLine("Передача файла " + text + " клиенту " + sender + " завершена.", _richEditControls[sender], _defaultFont, _positiveColor, ParagraphAlignment.Left);

                        progressBarControl1.Visible = false;
                        CancelBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                case MessageType.FileTransferAllowed:
                    {
                        AppendLine("Клиент одобрил получение файла.", _richEditControls[sender], _defaultFont, _positiveColor, ParagraphAlignment.Left);
                        
                        progressBarControl1.Visible = true;
                        progressBarControl1.EditValue = 0;
                        timer1.Enabled = true;
                        CancelBut.Visible = true;
                    }
                    break;

                case MessageType.FileTransferDenied:
                    {
                        AppendLine("Клиент отклонил получение файла.", _richEditControls[sender], _defaultFont, _negativeColor, ParagraphAlignment.Left);

                        progressBarControl1.Visible = false;
                        progressBarControl1.EditValue = 0;
                        timer1.Enabled = false;
                        CancelBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                case MessageType.FileReceivingStarted:
                    {
                        AppendLine("Начат прием файла. ", _richEditControls[sender], _defaultFont, _positiveColor, ParagraphAlignment.Left);

                        progressBarControl1.Visible = true;
                        progressBarControl1.EditValue = 0;
                        timer1.Enabled = true;
                        CancelBut.Visible = true;
                        FileBut.Visible = false;
                    }
                    break;

                case MessageType.FileTransferCanceledRecipientSide:
                    {
                        AppendLine("Прием файла отменен.", _richEditControls[sender], _defaultFont, _negativeColor, ParagraphAlignment.Left);

                        // Функция скрытия элементов приема файла
                        AllowBut.Visible = false;
                        DenyBut.Visible = false;
                        CancelBut.Visible = false;
                        progressBarControl1.Visible = false;
                        progressBarControl1.EditValue = 0;
                        timer1.Enabled = false;
                        FileBut.Visible = true;

                    }
                    break;

                case MessageType.FileTransferCanceledSenderSide:
                    {
                        AppendLine("Передача файла отменена.", _richEditControls[sender], _defaultFont, _negativeColor, ParagraphAlignment.Left);

                        // Скрытие элементов приема файла
                        AllowBut.Visible = false;
                        DenyBut.Visible = false;
                        CancelBut.Visible = false;
                        progressBarControl1.Visible = false;
                        progressBarControl1.EditValue = 0;
                        timer1.Enabled = false;
                        FileBut.Visible = true;

                    }
                    break;

                case MessageType.FileTransferCanceledBySender:
                    {
                        AppendLine("Передача файла отменена отправителем.", _richEditControls[sender], _defaultFont, _negativeColor, ParagraphAlignment.Left);

                        progressBarControl1.Visible = false;
                        progressBarControl1.EditValue = 0;
                        timer1.Enabled = false;
                        CancelBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                case MessageType.FileTransferCanceledByRecipient:
                    {
                        AppendLine("Передача файла отменена получателем.", _richEditControls[sender], _defaultFont, _negativeColor, ParagraphAlignment.Left);

                        progressBarControl1.EditValue = 0;
                        progressBarControl1.Visible = false;
                        timer1.Enabled = false;
                        CancelBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                case MessageType.FileReceivingTimeOut:
                    {
                        AppendLine("Вышло время ожидания файла " + text + " передача отменена.", _richEditControls[sender], _defaultFont, _negativeColor, ParagraphAlignment.Left);

                        progressBarControl1.Visible = false;
                        progressBarControl1.EditValue = 0;
                        timer1.Enabled = false;
                        CancelBut.Visible = false;
                        FileBut.Visible = true;
                        FileRequestLabel.Visible = false;
                        AllowBut.Visible = false;
                        DenyBut.Visible = false;
                    }
                    break;

                case MessageType.FileRequestCanceledRecipientSide:
                    {
                        AppendLine("Клиент отменил запрос на передачу файла.", _richEditControls[sender], _defaultFont, _negativeColor, ParagraphAlignment.Left);

                        FileRequestLabel.Visible = false;
                        AllowBut.Visible = false;
                        DenyBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                case MessageType.FileRequestCanceledSenderSide:
                    {
                        AppendLine("Запрос на передачу файла отменен.", _richEditControls[sender], _defaultFont, _negativeColor, ParagraphAlignment.Left);

                        FileRequestLabel.Visible = false;
                        AllowBut.Visible = false;
                        DenyBut.Visible = false;
                        CancelBut.Visible = false;
                        FileBut.Visible = true;
                        progressBarControl1.Visible = false;
                    }
                    break;

                case MessageType.ReadPortAvailable:
                    {
                        ReadPortValueLabelControl.Text  = "Доступен";
                        
                        return;
                    }
                    break;

                case MessageType.ReadPortUnavailable:
                    {
                        ReadPortValueLabelControl.Text = "Не доступен";
                        return;
                    }
                    break;

                case MessageType.WritePortAvailable:
                    {
                        WritePortValueLabelControl.Text = "Доступна";
                        return;
                    }
                    break;

                case MessageType.WritePortUnavailable:
                    {
                        WritePortValueLabelControl.Text = "Не доступна";
                        return;
                    }
                    break;

                case MessageType.WaitFileRecipientAnswer:
                    {
                        AppendLine("Ожидается ответ клиента о передачи файла ", _richEditControls[sender], _defaultFont, _systemColor, ParagraphAlignment.Left);

                    }
                    break;

                case MessageType.Error:
                    {
                        AppendLine(text, _richEditControls[sender], _defaultFont, _negativeColor, ParagraphAlignment.Left);
                    }
                    break;
            }

            try
            {
                _richEditControls[sender].Document.CaretPosition = _richEditControls[sender].Document.Range.End;
                _richEditControls[sender].ScrollToCaret();
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

           // _richEditControls[e.Sender].SelectionAlignment = HorizontalAlignment.Left;
           // _richEditControls[e.Sender].SelectionColor = Color.SteelBlue;
           // _richEditControls[e.Sender].AppendText("Прниять файл " + e.FileName + " размером " + filesize.ToString("0.00") + "МБ?" + '\n');

            AppendLine("Прниять файл " + e.FileName + " размером " + filesize.ToString("0.00") + "МБ?", _richEditControls[e.Sender], _defaultFont, _systemColor, ParagraphAlignment.Left);



            // Скрол вниз
           // if (_richEditControls[e.Sender].Document.Text.Length > 0)
           //     _richEditControls[e.Sender].Select(_richEditControls[e.Sender].TextLength - 1, 0);
            //_richEditControls[e.Sender].ScrollToCaret();

            try
            {
                _richEditControls[e.Sender].ScrollToCaret();
            }
            catch (Exception)
            {

            }

            FileBut.Visible = false;

            AllowBut.Visible = true;
            DenyBut.Visible = true;
            FileRequestLabel.Visible = true;

            if (Convert.ToByte(xtraTabControl1.SelectedTabPage.Tag) != e.Sender)
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            float receivedPacketPercentage;

            if (_cu.IsRecivingFile)
            {
                receivedPacketPercentage = (float)_cu.ProcessedFileSize / _cu.ReceivingFileSize * 100;
            }
            else
            {
                receivedPacketPercentage = (float)_cu.ProcessedFileSize / _cu.FileToTransfer.Length * 100;
            }

            if (receivedPacketPercentage < 100)
            {
                progressBarControl1.EditValue = (int)receivedPacketPercentage;
            }
            else
            {
                progressBarControl1.EditValue = 100;
            }
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

        private void XtraForm1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cu != null)
            {
                _cu.Dispose();
            }

           HideKeyBoard();
        }

        private void SendBut_Click(object sender, EventArgs e)
        {
            if (_cu.SendTextMessage(writeRichEditControl.Text, Convert.ToByte(xtraTabControl1.SelectedTabPage.Tag)))
            {
                writeRichEditControl.Document.Delete(writeRichEditControl.Document.Range); 
            }
            writeRichEditControl.Focus();
        }

        private void AllowBut_Click(object sender, EventArgs e)
        {
            writeRichEditControl.Focus();
            AllowBut.Visible = false;
            DenyBut.Visible = false;
            FileRequestLabel.Visible = false;
            progressBarControl1.Visible = true;
            //CanBut.Visible = true;
            _cu.AllowFileTransfer(); 
        }

        private void DenyBut_Click(object sender, EventArgs e)
        {
            writeRichEditControl.Focus();
            AllowBut.Visible = false;
            DenyBut.Visible = false;
            FileRequestLabel.Visible = false;
            _cu.DenyFileTransfer();
        }

        private void CancelBut_Click(object sender, EventArgs e)
        {
            writeRichEditControl.Focus();
            _cu.CancelSendingFile();
            _cu.CancelRecivingFile();
            _cu.CancleFileRequest();
            progressBarControl1.EditValue = 0;
            timer1.Enabled = false;
            CancelBut.Visible = false;
            FileBut.Visible = true;
        }

        private void FileBut_Click(object sender, EventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog();

            if (od.ShowDialog() == DialogResult.OK)
            {
                _cu.SendFileTransferRequest(od.FileName, Convert.ToByte(xtraTabControl1.SelectedTabPage.Tag));
                CancelBut.Visible = true;
                FileBut.Visible = false;
            }
            writeRichEditControl.Focus();
        }

        private void xtraTabControl1_SelectedPageChanged(object sender, TabPageChangedEventArgs e)
        {

            xtraTabControl1.SelectedTabPage.Text = "Клиент " + xtraTabControl1.SelectedTabPage.Tag;
            //MessageBox.Show(xtraTabControl1.SelectedTabPage.TabIndex.ToString());
            _newMessageCount[(int)xtraTabControl1.SelectedTabPage.Tag] = 0;
        }

        private void AppendLine(string text, RichEditControl richEditControl, Font font, Color color, ParagraphAlignment aligment)
        {
            
            DocumentRange range = richEditControl.Document.AppendText(text + '\n');

            CharacterProperties cp = richEditControl.Document.BeginUpdateCharacters(range);
            cp.FontName = font.Name;
            cp.FontSize = font.Size;
            cp.ForeColor = color;
            richEditControl.Document.EndUpdateCharacters(cp);

            ParagraphProperties pp = richEditControl.Document.BeginUpdateParagraphs(range);
            pp.Alignment = aligment;
            richEditControl.Document.EndUpdateParagraphs(pp);

        }

        private void AddHyperLink(string text, string uri, RichEditControl richEditControl, Font font)
        {
            DocumentRange range = richEditControl.Document.AppendText(text + '\n');

            CharacterProperties cp = richEditControl.Document.BeginUpdateCharacters(range);
            cp.FontName = font.Name;
            cp.FontSize = font.Size;
            richEditControl.Document.EndUpdateCharacters(cp);

            Hyperlink hyperlink = richEditControl.Document.CreateHyperlink(range);
            hyperlink.NavigateUri = uri;
        }

       // Не работает
       // private void AddImageHyperLink(string uri, RichEditControl richEditControl)
       // {
       //
       //         // Ресайз картинки 
       //         Image img = ResizeImg(Image.FromFile(uri), 320, 240);
       //     
       //         int startPos = richEditControl.Document.Range.End.ToInt();
       //     
       //         richEditControl.Document.InsertImage(richEditControl.Document.Range.End, img);
       //     
       //         int endPos = richEditControl.Document.Range.End.ToInt();
       //     
       //         DocumentRange range = richEditControl.Document.CreateRange(startPos, endPos);
       //     
       //         var images = richEditControl.Document.GetImages(range);
       //         
       //         
       //     
       //     
       //         // Добавляет пустую строку для отступа
       //         richEditControl.Document.AppendText("" + '\n');
       //     
       //     
       //     
       //         Hyperlink hyperlink = richEditControl.Document.CreateHyperlink(images[0].Range);
       //         hyperlink.NavigateUri = uri;
       //
       // }

        private void richEditControl1_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            e.Menu.Items.Clear();
        }

        void HideContextMenu (object sender, PopupMenuShowingEventArgs e)
        {
            e.Menu.Items.Clear();
        }

        private void writeRichEditControl_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && FileBut.Visible)
            {
                string[] docPath = (string[])e.Data.GetData(DataFormats.FileDrop);

                _cu.SendFileTransferRequest(docPath[0], Convert.ToByte(xtraTabControl1.SelectedTabPage.Tag));
                CancelBut.Visible = true;
                FileBut.Visible = false;
            }
            writeRichEditControl.Document.Delete(writeRichEditControl.Document.Range);
        }

        private void writeRichEditControl_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void writeRichEditControl_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void writeRichEditControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (_cu.SendTextMessage(writeRichEditControl.Text, Convert.ToByte(xtraTabControl1.SelectedTabPage.Tag)))
                {
                    _needClearWriteRichEditControl = true;
                }
            }
        }

        private void writeRichEditControl_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void writeRichEditControl_Click(object sender, EventArgs e)
        {            
             ShowKeyboard ();
        }

        private void writeRichEditControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return && _needClearWriteRichEditControl)
            {
                writeRichEditControl.Document.Delete(writeRichEditControl.Document.Range);
                _needClearWriteRichEditControl = false;
            }
            

        }

        private void XtraForm1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                HideKeyBoard();
            }           
        }

        private void ButClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ButMin_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
      
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private const uint WM_COMMAND = 0x0111;

        private void ShowKeyboard ()
        {
            Boolean KeyboardOpen = false;

            foreach (Process process in Process.GetProcessesByName("TabTip"))
            {
                KeyboardOpen = true;
            }

            if (KeyboardOpen)
            {
                KeybordBottomDock();
            }
            else
            {
                if (File.Exists(@"C:\Program Files\Common Files\microsoft shared\ink\tabtip.exe"))
                {
                    Process.Start(@"C:\Program Files\Common Files\microsoft shared\ink\tabtip.exe");
                    KeybordBottomDock();
                }
                else
                {
                    MessageBox.Show("Приложение клавиатуры не найдено.");
                }
            }
        }

        private void KeybordBottomDock ()
        {
            var wKB = FindWindow("IPTip_Main_Window", null);

            PostMessage(wKB, WM_COMMAND, 10021, 0);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint RegisterWindowMessage(string lpString);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        private void HideKeyBoard ()
        {
           // Process[] processlist = Process.GetProcesses();
           //
           // foreach (Process process in processlist)
           // {
           //     if (process.ProcessName == "TabTip")
           //     {
           //         process.Kill();
           //         break;
           //     }
           // }

            var WM_DESKBAND_CLICKED = RegisterWindowMessage("TabletInputPanelDeskBandClicked");

            var wKB = FindWindow("IPTip_Main_Window", null);
            if( wKB != null && IsWindowVisible(wKB))
            {
               PostMessage(wKB, WM_DESKBAND_CLICKED, 0, 0);
            }

        }

        private void xtraTabControl1_Click(object sender, EventArgs e)
        {
            writeRichEditControl.Focus();
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panelControl1_Paint(object sender, PaintEventArgs e)
        {

        }


       

    }
}