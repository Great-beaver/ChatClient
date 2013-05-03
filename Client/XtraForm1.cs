using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Chat.Main;
using ChatClient.Main;
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

        Font _defaultFont = new Font("Tahoma",8);
        Font _deliveryFont = new Font("Microsoft Sans Serif", 7);


        Color _positiveColor = Color.Green;
        Color _negativeColor = Color.Red;
        Color _neutralColor = Color.Black;
        Color _systemColor = Color.SteelBlue;

        int[] _newMessageCount = new int[5];


        private void XtraForm1_Load(object sender, EventArgs e)
        {
           for (int i = 0; i < _newMessageCount.Length; i++)
           {
               _newMessageCount[i] = 0;
           }
          

            _cu = new CommunicationUnit(Properties.Settings.Default.ReadPort, Properties.Settings.Default.WritePort, 
                  Properties.Settings.Default.ClientId, Properties.Settings.Default.PortSpeed);
              // Подписывание на событие

            _cu.MessageRecived += new EventHandler<MessageRecivedEventArgs>(ComPortMessageRecived);
            _cu.FileRequestRecived += new EventHandler<FileRequestRecivedEventArgs>(ComPortFileRequestRecived);

            barStaticIdValue.Caption = _cu.ClietnId.ToString();
            
            for (int i = 0; i < 5; i++)
            {
                _richEditControls[i] = new RichEditControl();
                if (i != _cu.ClietnId)
                {
                    //string title = "TabPage " + (tabControl1.TabCount + 1).ToString();
                    _tabPages[i] = new XtraTabPage();
                    _tabPages[i].Text = "Клиент " + i;
                    _tabPages[i].Tag = i;

                    _richEditControls[i].Name = "ClientRichEditControl" + i;
                    _richEditControls[i].Left = 1;
                    _richEditControls[i].Top = 1;
                    _richEditControls[i].Width = xtraTabControl1.Width - 5;
                    _richEditControls[i].Height = xtraTabControl1.Height - 28;
                    _richEditControls[i].Options.HorizontalRuler.Visibility = RichEditRulerVisibility.Hidden;
                    _richEditControls[i].Options.HorizontalScrollbar.Visibility = RichEditScrollbarVisibility.Hidden;
                    _richEditControls[i].Options.VerticalRuler.Visibility = RichEditRulerVisibility.Hidden;
                    _richEditControls[i].ActiveViewType = RichEditViewType.Simple;
                    _richEditControls[i].Views.SimpleView.Padding = new Padding(5,4,4,0);
                    _richEditControls[i].ReadOnly = true;
                    _richEditControls[i].ShowCaretInReadOnly = false;
                    _richEditControls[i].PopupMenuShowing += new PopupMenuShowingEventHandler(HideContextMenu);
                    
                    //_richEditControls[i].BackColor = Color.Beige;

                    //  _richTextBoxs[i].Font = new Font("Microsoft Sans Serif", 10);

                    //_richTextBoxs[i].AppendText(_tabPages[i].TabIndex.ToString());

                    _tabPages[i].Controls.Add(_richEditControls[i]);

                    xtraTabControl1.TabPages.Add(_tabPages[i]);
                }
            }  



            writeRichEditControl.AllowDrop = true;
            writeRichEditControl.DragDrop += new DragEventHandler(writeRichEditControl_DragDrop);
            writeRichEditControl.DragEnter += new DragEventHandler(writeRichEditControl_DragEnter);
            writeRichEditControl.DragOver += new DragEventHandler(writeRichEditControl_DragOver);
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
                      //  _richEditControls[sender].SelectionAlignment = HorizontalAlignment.Left;
                      //  _richEditControls[sender].SelectionColor = Color.Black;
                      //  _richEditControls[sender].AppendText(text + '\n');

                        AppendLine(text, _richEditControls[sender], _defaultFont, _neutralColor, ParagraphAlignment.Left);

                        if (Convert.ToByte(xtraTabControl1.SelectedTabPage.Tag) != sender)
                        {
                            _tabPages[sender].Text = "Клиент " + sender + " +" + ++_newMessageCount[sender];
                        }

                    }
                    break;

                case MessageType.TextDelivered:
                    {
                      // _richEditControls[sender].SelectionAlignment = HorizontalAlignment.Right;
                      // _richEditControls[sender].SelectionColor = Color.Black;
                      // _richEditControls[sender].AppendText(text + '\n');

                        AppendLine(text, _richEditControls[sender], _defaultFont, _neutralColor, ParagraphAlignment.Right);


                      //  _richEditControls[sender].SelectionColor = Color.Green;
                      //  _richEditControls[sender].SelectionFont = new Font("Microsoft Sans Serif", 7);
                      //  _richEditControls[sender].AppendText("Доставлено." + '\n');

                        AppendLine("Доставлено.", _richEditControls[sender], _deliveryFont, _positiveColor, ParagraphAlignment.Right);

                    }
                    break;

                case MessageType.TextUndelivered:
                    {
                       // _richEditControls[sender].SelectionAlignment = HorizontalAlignment.Right;
                       // _richEditControls[sender].SelectionColor = Color.Black;
                       // _richEditControls[sender].AppendText(text + '\n');

                        AppendLine(text, _richEditControls[sender], _defaultFont, _neutralColor, ParagraphAlignment.Right);

                       // _richEditControls[sender].SelectionColor = Color.Red;
                       // _richEditControls[sender].SelectionFont = new Font("Microsoft Sans Serif", 7);
                       // _richEditControls[sender].AppendText("Не доставлено." + '\n');

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
                      //  _richEditControls[sender].SelectionAlignment = HorizontalAlignment.Left;
                      //  _richEditControls[sender].SelectionColor = Color.DarkRed;
                      //  _richEditControls[sender].AppendText("Получатель " + sender + " не доступен, отправка файла отменена." + '\n');

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
                     //  _richEditControls[sender].SelectionAlignment = HorizontalAlignment.Left;
                     //  _richEditControls[sender].SelectionColor = Color.DarkRed;
                     //  _richEditControls[sender].AppendText("Клиента " + sender + " не доступен." + '\n');

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
                      // _richEditControls[sender].SelectionAlignment = HorizontalAlignment.Left;
                      // _richEditControls[sender].SelectionColor = Color.DarkGreen;
                      // _richEditControls[sender].AppendText("Файл " + text + " от клиента " + sender + " получен." + '\n');

                        AppendLine("Файл " + text + " от клиента " + sender + " получен.", _richEditControls[sender], _defaultFont, _negativeColor, ParagraphAlignment.Left);

                        progressBarControl1.Visible = false;
                        progressBarControl1.EditValue = 0;
                        timer1.Enabled = false;
                        CancelBut.Visible = false;
                        FileBut.Visible = true;

                        // Сохраняет содержимое буффера
                        IDataObject tmp = Clipboard.GetDataObject();


                        // Разрешить добавление в richtextbox элементов из буфера
                        _richEditControls[sender].ReadOnly = false;

                        // Проверка является ли файл изображением
                        string e = Path.GetExtension(_cu.ReceivingFileFullName);

                        if (e == ".jpg" || e == ".bmp")
                        {
                            // Вывод изображения
                            Image img = ResizeImg(Image.FromFile(_cu.ReceivingFileFullName), 80, 60);

                            Clipboard.SetImage(img);

                            _richEditControls[sender].Paste();
                        }

                        // Вставляет файл в richtextv
                        StringCollection paths = new StringCollection();
                        paths.Add(_cu.ReceivingFileFullName);
                        Clipboard.SetFileDropList(paths);
                        _richEditControls[sender].Paste();

                        // Запретить вставку элементов
                        _richEditControls[sender].ReadOnly = true;

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
                       // _richEditControls[sender].AppendText("" + '\n');

                        _richEditControls[sender].Document.AppendText(""+'\n');

                    }
                    break;

                case MessageType.FileSendingComplete:
                    {
                       // _richEditControls[sender].SelectionAlignment = HorizontalAlignment.Left;
                       // _richEditControls[sender].SelectionColor = Color.DarkGreen;
                       // _richEditControls[sender].AppendText("Передача файла " + text + " клиенту " + sender + " завершена." + '\n');

                        AppendLine("Передача файла " + text + " клиенту " + sender + " завершена.", _richEditControls[sender], _defaultFont, _positiveColor, ParagraphAlignment.Left);

                        progressBarControl1.Visible = false;
                        CancelBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                case MessageType.FileTransferAllowed:
                    {
                       // _richEditControls[sender].SelectionAlignment = HorizontalAlignment.Left;
                       // _richEditControls[sender].SelectionColor = Color.DarkGreen;
                       // _richEditControls[sender].AppendText("Клиент одобрил получение файла." + '\n');

                        AppendLine("Клиент одобрил получение файла.", _richEditControls[sender], _defaultFont, _positiveColor, ParagraphAlignment.Left);
                        
                        progressBarControl1.Visible = true;
                        progressBarControl1.EditValue = 0;
                        timer1.Enabled = true;
                        CancelBut.Visible = true;
                    }
                    break;

                case MessageType.FileTransferDenied:
                    {
                       // _richEditControls[sender].SelectionAlignment = HorizontalAlignment.Left;
                       // _richEditControls[sender].SelectionColor = Color.DarkRed;
                       // _richEditControls[sender].AppendText("Клиент отклонил получение файла." + '\n');

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
                       // _richEditControls[sender].SelectionAlignment = HorizontalAlignment.Left;
                       // _richEditControls[sender].SelectionColor = Color.DarkGreen;
                       // _richEditControls[sender].AppendText("Начат прием файла  " + text + "." + '\n');

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
                        //_richEditControls[sender].SelectionAlignment = HorizontalAlignment.Left;
                        //_richEditControls[sender].SelectionColor = Color.DarkRed;
                        //_richEditControls[sender].AppendText("Прием файла отменен." + '\n');

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
                       //_richEditControls[sender].SelectionAlignment = HorizontalAlignment.Left;
                       //_richEditControls[sender].SelectionColor = Color.DarkRed;
                       //_richEditControls[sender].AppendText( "Передачай файла отменена." + '\n');

                        AppendLine("Передачай файла отменена.", _richEditControls[sender], _defaultFont, _negativeColor, ParagraphAlignment.Left);

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

                case MessageType.FileTransferCanceledBySender:
                    {
                       // _richEditControls[sender].SelectionAlignment = HorizontalAlignment.Left;
                       // _richEditControls[sender].SelectionColor = Color.DarkRed;
                       // _richEditControls[sender].AppendText("Передача файла отменена отправителем." + '\n');

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
                       // _richEditControls[sender].SelectionAlignment = HorizontalAlignment.Left;
                       // _richEditControls[sender].SelectionColor = Color.DarkRed;
                       // _richEditControls[sender].AppendText( "Передача файла отменена получателем." + '\n');

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
                       // _richEditControls[sender].SelectionAlignment = HorizontalAlignment.Left;
                       // _richEditControls[sender].SelectionColor = Color.DarkRed;
                       // _richEditControls[sender].AppendText("Вышло время ожидания файла " + text + " передача отменена." + '\n');

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
                       // _richEditControls[sender].SelectionAlignment = HorizontalAlignment.Left;
                       // _richEditControls[sender].SelectionColor = Color.DarkRed;
                       // _richEditControls[sender].AppendText("Клиент отменил запрос на передачу файла." + '\n');

                        AppendLine("Клиент отменил запрос на передачу файла.", _richEditControls[sender], _defaultFont, _negativeColor, ParagraphAlignment.Left);

                        FileRequestLabel.Visible = false;
                        AllowBut.Visible = false;
                        DenyBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                case MessageType.FileRequestCanceledSenderSide:
                    {
                        //_richEditControls[sender].SelectionAlignment = HorizontalAlignment.Left;
                        //_richEditControls[sender].SelectionColor = Color.DarkRed;
                        //_richEditControls[sender].AppendText("Запрос на передачу файла отменен." + '\n');

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
                        barStaticReadPortValue.Caption = "Online";
                      //  StatusLabelReadPortValue.ForeColor = Color.Green;
                        return;
                    }
                    break;

                case MessageType.ReadPortUnavailable:
                    {
                        barStaticReadPortValue.Caption = "Offline";
                     //   StatusLabelReadPortValue.ForeColor = Color.Red;
                        return;
                    }
                    break;

                case MessageType.WritePortAvailable:
                    {
                        barStaticWritePortValue.Caption = "Online";
                     //   StatusLabelWritePortValue.ForeColor = Color.Green;
                        return;
                    }
                    break;

                case MessageType.WritePortUnavailable:
                    {
                        barStaticWritePortValue.Caption = "Offline";
                      //  StatusLabelWritePortValue.ForeColor = Color.Red;
                        return;
                    }
                    break;

                case MessageType.WaitFileRecipientAnswer:
                    {
                        //_richEditControls[sender].SelectionAlignment = HorizontalAlignment.Left;
                        //_richEditControls[sender].SelectionColor = Color.SteelBlue;
                        //_richEditControls[sender].AppendText("Ожидается ответ клиента о передачи файла " + text + "." + '\n');

                        AppendLine("Ожидается ответ клиента о передачи файла ", _richEditControls[sender], _defaultFont, _systemColor, ParagraphAlignment.Left);

                    }
                    break;

                case MessageType.Error:
                    {
                       // _richEditControls[sender].SelectionColor = Color.Red;
                       // _richEditControls[sender].AppendText(text + '\n');

                        AppendLine(text, _richEditControls[sender], _defaultFont, _negativeColor, ParagraphAlignment.Left);
                    }
                    break;
            }

            try
            {
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
        }

        private void SendBut_Click(object sender, EventArgs e)
        {
            if (_cu.SendTextMessage(writeRichEditControl.Text, Convert.ToByte(xtraTabControl1.SelectedTabPage.Tag)))
            {
                writeRichEditControl.Document.Delete(writeRichEditControl.Document.Range);
            } 
        }

        private void AllowBut_Click(object sender, EventArgs e)
        {
            AllowBut.Visible = false;
            DenyBut.Visible = false;
            FileRequestLabel.Visible = false;
            progressBarControl1.Visible = true;
            //CanBut.Visible = true;
            _cu.AllowFileTransfer(); 
        }

        private void DenyBut_Click(object sender, EventArgs e)
        {
            AllowBut.Visible = false;
            DenyBut.Visible = false;
            FileRequestLabel.Visible = false;
            _cu.DenyFileTransfer();
        }

        private void CancelBut_Click(object sender, EventArgs e)
        {
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
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                if (_cu.SendTextMessage(writeRichEditControl.Text, Convert.ToByte(xtraTabControl1.SelectedTabPage.Tag)))
                {
                    writeRichEditControl.Document.Delete(writeRichEditControl.Document.Range);
                }
            }
        }

        private void writeRichEditControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && writeRichEditControl.Document.Text== "")
            {
                writeRichEditControl.Document.Delete(writeRichEditControl.Document.Range);
            }
        }



    }
}