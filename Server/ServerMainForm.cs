using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AxAXISMEDIACONTROLLib;
using Chat.Main;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using DevExpress.XtraTab;

namespace Server
{
    public partial class ServerMainForm : XtraForm
    {
        public ServerMainForm()
        {
            InitializeComponent();
        }

        CommunicationUnit _cu;
        private Dictionary<int, XtraTabPage>  _tabPages; // = new XtraTabPage[5];
        private Dictionary<int, RichEditControl> _richEditControls; // = new RichEditControl[5];
        private Dictionary<int, string> _tabsNames; // = new string[5];
        private Dictionary<int, int> _newMessageCount; // = new int[5];
        AxAxisMediaControl[] AMCs  = new AxAxisMediaControl[4];
        PanelControl[] VideoPanels = new PanelControl[4];

        // ID для широковещательныйх сообщенмй равный ID клиента
        private int _broadcastId; 

        Font _defaultFont = new Font("Tahoma",12);

        Font _deliveryFont = new Font("Microsoft Sans Serif", 8);

        Font _headerFont = new Font("Tahoma", 11,FontStyle.Bold);

        Font _buttonFont = new Font("Tahoma", 10, FontStyle.Bold);

        private Boolean _needClearWriteRichEditControl = false;
            
        Color _positiveColor = Color.Green;
        Color _negativeColor = Color.Red;
        Color _neutralColor = Color.Black;
        Color _systemColor = Color.SteelBlue;
        Color _ClientNameColor = Color.Gray;

        string[] _quickCommands = Properties.Settings.Default.QucikCommands.Split('|');

        BarStaticItem[] _readPortsValuesStaticItems = new BarStaticItem[4];

        BarStaticItem _writePortValueStaticitem = new BarStaticItem();

        BarStaticItem _IdValueStaticitem = new BarStaticItem();

        private BarStaticItem NewBarStaticItem (string caption, BarItemLinkAlignment aligment = BarItemLinkAlignment.Left)
        {
            BarStaticItem item = new BarStaticItem();
            item.Caption = caption;
            item.Alignment = aligment;
            return item;
        }

        private LabelControl NewVideoWindowLabelControl (string text)
        {
            LabelControl item = new LabelControl();
            item.Text = text;
            item.BackColor = Color.FromArgb( 45, 45, 45);
            item.Font = new Font("Tahoma", 15, FontStyle.Bold);
            return item;
        }

        private void StatusBarInitialize() 
        {
            bar1.AddItem(NewBarStaticItem("Прием:"));

            for (int i = 0; i < _readPortsValuesStaticItems.Length; i++)
            {
                bar1.AddItem(NewBarStaticItem("Порт " + (i + 1)));
                _readPortsValuesStaticItems[i] = NewBarStaticItem("Value" + (i + 1));
                bar1.AddItem(_readPortsValuesStaticItems[i]);
            }

            bar1.AddItem(NewBarStaticItem("Передача:"));

            _writePortValueStaticitem.Caption = "Value";
            bar1.AddItem(_writePortValueStaticitem);


            bar1.AddItem(NewBarStaticItem("ID:"));
            _IdValueStaticitem.Caption = "Value";
            bar1.AddItem(_IdValueStaticitem);

            BarButtonItem settingsButton = new BarButtonItem();
            settingsButton.Caption = "Настройки";
            settingsButton.Alignment = BarItemLinkAlignment.Right;
            settingsButton.ItemClick += (ea, s) =>
                                            {
                                                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                                                using (var settingsForm = new SettingsForm(new Setting
                                                    {
                                                        WindowEnabled1 = Convert.ToBoolean(config.AppSettings.Settings["EnabledVideoWindow1"].Value),
                                                        WindowEnabled2 = Convert.ToBoolean(config.AppSettings.Settings["EnabledVideoWindow2"].Value),
                                                        WindowEnabled3 = Convert.ToBoolean(config.AppSettings.Settings["EnabledVideoWindow3"].Value),
                                                        WindowEnabled4 = Convert.ToBoolean(config.AppSettings.Settings["EnabledVideoWindow4"].Value)
                                                    }))
                                                {
                                                    if (settingsForm.ShowDialog(this) == DialogResult.OK)
                                                    {
                                                        config.AppSettings.Settings["EnabledVideoWindow1"].Value = settingsForm.Setting.WindowEnabled1.ToString();
                                                        config.AppSettings.Settings["EnabledVideoWindow2"].Value = settingsForm.Setting.WindowEnabled2.ToString();
                                                        config.AppSettings.Settings["EnabledVideoWindow3"].Value = settingsForm.Setting.WindowEnabled3.ToString();
                                                        config.AppSettings.Settings["EnabledVideoWindow4"].Value = settingsForm.Setting.WindowEnabled4.ToString();
                                                        config.Save(ConfigurationSaveMode.Modified);
                                                        ConfigurationManager.RefreshSection("appSettings");
                                                        ShowVideoWindows();
                                                    }   
                                                }
                                            };
           

            bar1.AddItem(settingsButton);
            
            if (_cu != null && _cu.ClietnId != null)
                
            _IdValueStaticitem.Caption = _cu.ClietnId.ToString();
        } 

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
                        writeRichEditControl.Document.AppendText(button.Text + ".");
                        writeRichEditControl.Focus();
                    }
                };
                flowLayoutPanel1.Controls.Add(b);
            }
        }

        private void RichEditControlsInitialize(int[] clients)
        {

            _tabPages = new Dictionary<int, XtraTabPage>();
            _richEditControls = new Dictionary<int, RichEditControl>();
            _tabsNames = new Dictionary<int, string>();
            _newMessageCount = new Dictionary<int, int>();

            foreach (int client in clients)
            {
                _newMessageCount.Add(client,0);
            }


            // Иницилизация вкладки "Все", ее ID равен ID клиента

            int all = _cu.ClietnId; // Хранит ClietnId в формате int так как при инициализации с переменой в byte возникают проблемы

            _richEditControls.Add(all, new RichEditControl());
            _tabPages.Add(all, new XtraTabPage());
            _tabPages[all].Text = "Все";
            _tabsNames.Add(all, "Все");
            _tabPages[all].Tag = all;
            _tabPages[all].Appearance.Header.Font = _headerFont;

            _richEditControls[all].Name = "ClientRichEditControl" + all;
            _richEditControls[all].Left = 1;
            _richEditControls[all].Top = 1;
            _richEditControls[all].Width = xtraTabControl1.Width - 5;
            _richEditControls[all].Height = xtraTabControl1.Height - 28;
            _richEditControls[all].Options.HorizontalRuler.Visibility = RichEditRulerVisibility.Hidden;
            _richEditControls[all].Options.HorizontalScrollbar.Visibility = RichEditScrollbarVisibility.Hidden;
            _richEditControls[all].Options.VerticalRuler.Visibility = RichEditRulerVisibility.Hidden;
            _richEditControls[all].Options.VerticalScrollbar.Visibility = RichEditScrollbarVisibility.Visible;
            _richEditControls[all].Options.Hyperlinks.ModifierKeys = Keys.None;
            _richEditControls[all].ActiveViewType = RichEditViewType.Simple;
            _richEditControls[all].Views.SimpleView.Padding = new Padding(5, 4, 4, 0);
            _richEditControls[all].ReadOnly = true;
            _richEditControls[all].ShowCaretInReadOnly = false;
            _richEditControls[all].Dock = DockStyle.Fill;
            _richEditControls[all].PopupMenuShowing += new PopupMenuShowingEventHandler(HideContextMenu);
            _richEditControls[all].Click += (s, ea) =>
            {
                writeRichEditControl.Focus();
            };

            _tabPages[all].Controls.Add(_richEditControls[all]);

            ////
            xtraTabControl1.TabPages.Add(_tabPages[all]);
            ////


            //for (int i = 0; i < clients.Length; i++)

            foreach (int client in clients)
            {
                if (client != _cu.ClietnId)
                {
                    _richEditControls.Add(client, new RichEditControl());
                    _tabPages.Add(client, new XtraTabPage());
                    _tabPages[client].Text = "Клиент " + client;
                    _tabsNames.Add(client, "Клиент " + client);
                    _tabPages[client].Tag = client;
                    _tabPages[client].Appearance.Header.Font = _headerFont;

                    _richEditControls[client].Name = "ClientRichEditControl" + client;
                    _richEditControls[client].Left = 1;
                    _richEditControls[client].Top = 1;
                    _richEditControls[client].Width = xtraTabControl1.Width - 5;
                    _richEditControls[client].Height = xtraTabControl1.Height - 28;
                    _richEditControls[client].Options.HorizontalRuler.Visibility = RichEditRulerVisibility.Hidden;
                    _richEditControls[client].Options.HorizontalScrollbar.Visibility = RichEditScrollbarVisibility.Hidden;
                    _richEditControls[client].Options.VerticalRuler.Visibility = RichEditRulerVisibility.Hidden;
                    _richEditControls[client].Options.VerticalScrollbar.Visibility = RichEditScrollbarVisibility.Visible;
                    _richEditControls[client].Options.Hyperlinks.ModifierKeys = Keys.None;
                    _richEditControls[client].ActiveViewType = RichEditViewType.Simple;
                    _richEditControls[client].Views.SimpleView.Padding = new Padding(5, 4, 4, 0);
                    _richEditControls[client].ReadOnly = true;
                    _richEditControls[client].ShowCaretInReadOnly = false;
                    _richEditControls[client].Dock = DockStyle.Fill;
                    _richEditControls[client].PopupMenuShowing += new PopupMenuShowingEventHandler(HideContextMenu);
                    _richEditControls[client].Click += (s, ea) =>
                    {
                        writeRichEditControl.Focus();
                    };

                    _tabPages[client].Controls.Add(_richEditControls[client]);

                    xtraTabControl1.TabPages.Add(_tabPages[client]);
                }


            }
            writeRichEditControl.AllowDrop = true;
            writeRichEditControl.DragDrop += new DragEventHandler(writeRichEditControl_DragDrop);
            writeRichEditControl.DragEnter += new DragEventHandler(writeRichEditControl_DragEnter);
            writeRichEditControl.DragOver += new DragEventHandler(writeRichEditControl_DragOver);
        }

        private void XtraForm1_Load(object sender, EventArgs e)
        {
           MinimumSize = Size;
           WindowState = FormWindowState.Maximized;
           writeRichEditControl.Font = _defaultFont;

          // for (int i = 0; i < _newMessageCount.Length; i++)
         //  foreach (var key in _newMessageCount.Keys.ToList())
         //  {
         //      _newMessageCount[key] = 0;
         //           //_newMessageCount[i] = 0;
         //   }
           
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

                        if (recipient != _cu.ClietnId)
                        {
                            AppendLine(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") +  "Отправлено клиенту " + recipient, _richEditControls[sender], _defaultFont, _systemColor, ParagraphAlignment.Center);
                            AppendLine(text, _richEditControls[sender], _defaultFont, _systemColor, ParagraphAlignment.Left);

                        }
                        else
                        {
                            AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + ":", sender), _richEditControls[sender], _deliveryFont, _ClientNameColor, ParagraphAlignment.Left);
                            AppendLine(text, _richEditControls[sender], _defaultFont, _neutralColor, ParagraphAlignment.Left);
                        }

                        if (Convert.ToByte(xtraTabControl1.SelectedTabPage.Tag) != sender)
                        {
                            _tabPages[sender].Text = "Клиент " + sender + " +" + ++_newMessageCount[sender];
                            return;
                        }

                    }
                    break;

                case MessageType.TextDelivered:
                    {
                        AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + ":", sender), _richEditControls[sender], _deliveryFont, _ClientNameColor, ParagraphAlignment.Right);
                        AppendLine(text, _richEditControls[sender], _defaultFont, _neutralColor, ParagraphAlignment.Right);

                        AppendLine("Доставлено.", _richEditControls[sender], _deliveryFont, _positiveColor, ParagraphAlignment.Right);

                    }
                    break;

                case MessageType.TextUndelivered:
                    {
                        AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + ":", sender), _richEditControls[sender], _deliveryFont, _ClientNameColor, ParagraphAlignment.Right);
                        AppendLine(text, _richEditControls[sender], _defaultFont, _neutralColor, ParagraphAlignment.Right);

                        AppendLine("Не доставлено.", _richEditControls[sender], _deliveryFont, _negativeColor, ParagraphAlignment.Right);

                    }
                    break;

                case MessageType.BroadcastText:
                    {
                        AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + " Клиент {0}:", sender), _richEditControls[_broadcastId], _deliveryFont, _ClientNameColor, ParagraphAlignment.Left);
                        AppendLine(text, _richEditControls[_cu.ClietnId], _defaultFont, _neutralColor, ParagraphAlignment.Left);

                        if (Convert.ToByte(xtraTabControl1.SelectedTabPage.Tag) != _cu.ClietnId)
                        {
                            _tabPages[_cu.ClietnId].Text = _tabsNames[_cu.ClietnId] + " +" + ++_newMessageCount[_cu.ClietnId];
                            return;
                        }

                    }
                    break;

                case MessageType.BroadcastTextDelivered:
                    {
                        AppendLine("Доставлено клиенту " + recipient, _richEditControls[_broadcastId], _deliveryFont, _positiveColor, ParagraphAlignment.Right);
                    }
                    break;

                case MessageType.BroadcastTextUndelivered:
                    {
                        AppendLine("Не доставлено клиенту " + recipient, _richEditControls[_broadcastId], _deliveryFont, _negativeColor, ParagraphAlignment.Right);
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
                        AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + ":", sender), _richEditControls[sender], _deliveryFont, _ClientNameColor, ParagraphAlignment.Left);
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
                        // BUG here 
                        AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + ":", sender), _richEditControls[sender], _deliveryFont, _ClientNameColor, ParagraphAlignment.Left);
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
                        AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + ":", sender), _richEditControls[sender], _deliveryFont, _ClientNameColor, ParagraphAlignment.Left);
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
                        AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + ":", sender), _richEditControls[sender], _deliveryFont, _ClientNameColor, ParagraphAlignment.Left);
                        AppendLine("Передача файла " + text + " клиенту " + sender + " завершена.", _richEditControls[sender], _defaultFont, _positiveColor, ParagraphAlignment.Left);

                        progressBarControl1.Visible = false;
                        CancelBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                case MessageType.FileTransferAllowed:
                    {
                        AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + ":", sender), _richEditControls[sender], _deliveryFont, _ClientNameColor, ParagraphAlignment.Left);
                        AppendLine("Клиент одобрил получение файла.", _richEditControls[sender], _defaultFont, _positiveColor, ParagraphAlignment.Left);
                        
                        progressBarControl1.Visible = true;
                        progressBarControl1.EditValue = 0;
                        timer1.Enabled = true;
                        CancelBut.Visible = true;
                    }
                    break;

                case MessageType.FileTransferDenied:
                    {
                        AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + ":", sender), _richEditControls[sender], _deliveryFont, _ClientNameColor, ParagraphAlignment.Left);
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
                        AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + ":", sender), _richEditControls[sender], _deliveryFont, _ClientNameColor, ParagraphAlignment.Left);
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
                        AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + ":", sender), _richEditControls[sender], _deliveryFont, _ClientNameColor, ParagraphAlignment.Left);
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
                        AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + ":", sender), _richEditControls[sender], _deliveryFont, _ClientNameColor, ParagraphAlignment.Left);
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
                        AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + ":", sender), _richEditControls[sender], _deliveryFont, _ClientNameColor, ParagraphAlignment.Left);
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
                        AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + ":", sender), _richEditControls[sender], _deliveryFont, _ClientNameColor, ParagraphAlignment.Left);
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
                        AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + ":", sender), _richEditControls[sender], _deliveryFont, _ClientNameColor, ParagraphAlignment.Left);
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
                        AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + ":", sender), _richEditControls[sender], _deliveryFont, _ClientNameColor, ParagraphAlignment.Left);
                        AppendLine("Клиент отменил запрос на передачу файла.", _richEditControls[sender], _defaultFont, _negativeColor, ParagraphAlignment.Left);

                        FileRequestLabel.Visible = false;
                        AllowBut.Visible = false;
                        DenyBut.Visible = false;
                        FileBut.Visible = true;
                    }
                    break;

                case MessageType.FileRequestCanceledSenderSide:
                    {
                        AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + ":", sender), _richEditControls[sender], _deliveryFont, _ClientNameColor, ParagraphAlignment.Left);
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
                        _readPortsValuesStaticItems[sender].Caption = "Доступен";
                        
                        return;
                    }
                    break;

                case MessageType.ReadPortUnavailable:
                    {
                        _readPortsValuesStaticItems[sender].Caption = "Не доступен";
                        return;
                    }
                    break;

                case MessageType.WritePortAvailable:
                    {
                        _writePortValueStaticitem.Caption = "Доступна";
                        return;
                    }
                    break;

                case MessageType.WritePortUnavailable:
                    {
                        _writePortValueStaticitem.Caption = "Не доступна";
                        return;
                    }
                    break;

                case MessageType.WaitFileRecipientAnswer:
                    {
                        AppendLine(String.Format(DateTime.Now.ToString("(HH:mm:ss dd.MM.yy)") + ":", sender), _richEditControls[sender], _deliveryFont, _ClientNameColor, ParagraphAlignment.Left);
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
        }

        private void SendBut_Click(object sender, EventArgs e)
        {
            if (Convert.ToByte(xtraTabControl1.SelectedTabPage.Tag) == _broadcastId)
            {
                if (!_cu.SendBroadcastTextMessage(writeRichEditControl.Text))
                {
                    return;
                }
                AppendLine(writeRichEditControl.Text, _richEditControls[_broadcastId], _defaultFont, _neutralColor, ParagraphAlignment.Right);
                _richEditControls[_broadcastId].Document.CaretPosition = _richEditControls[_broadcastId].Document.Range.End;
                _richEditControls[_broadcastId].ScrollToCaret();
                writeRichEditControl.Document.Delete(writeRichEditControl.Document.Range);

            }
            else
            {
                if (_cu.SendTextMessage(writeRichEditControl.Text, Convert.ToByte(xtraTabControl1.SelectedTabPage.Tag)))
                {
                    writeRichEditControl.Document.Delete(writeRichEditControl.Document.Range);
                }
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
            xtraTabControl1.SelectedTabPage.Text = _tabsNames[(int)xtraTabControl1.SelectedTabPage.Tag];
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
                if (Convert.ToByte(xtraTabControl1.SelectedTabPage.Tag) == _broadcastId)
                {
                    if (!_cu.SendBroadcastTextMessage(writeRichEditControl.Text))
                    {
                        return;
                    }
                    AppendLine(writeRichEditControl.Text, _richEditControls[_broadcastId], _defaultFont, _neutralColor, ParagraphAlignment.Right);
                    _richEditControls[_broadcastId].Document.CaretPosition = _richEditControls[_broadcastId].Document.Range.End;
                    _richEditControls[_broadcastId].ScrollToCaret();
                    _needClearWriteRichEditControl = true;
                }
                else
                {
                    if (_cu.SendTextMessage(writeRichEditControl.Text, Convert.ToByte(xtraTabControl1.SelectedTabPage.Tag)))
                    {
                        _needClearWriteRichEditControl = true;
                    }
                }
                writeRichEditControl.Focus();
            }
        }

        private void writeRichEditControl_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void writeRichEditControl_Click(object sender, EventArgs e)
        {            
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
            
        }

        private void xtraTabControl1_Click(object sender, EventArgs e)
        {
            writeRichEditControl.Focus();
        }

        private void panelControl2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void AMCInitialize()
        {
            for (int i = 0; i < AMCs.Length; i++)
            {
                AMCs[i] = new AxAxisMediaControl();
                AMCs[i].Tag = i;
                AMCs[i].OnDoubleClick += (s, ea) =>
                {
                    if (s is AxAxisMediaControl)
                    {
                        var amc = s as AxAxisMediaControl;
                        amc.FullScreen = !amc.FullScreen;
                    }
                };

                AMCs[i].OnClick += (s, ea) =>
                                       {
                                           if (s is AxAxisMediaControl)
                                           {
                                               var amc = s as AxAxisMediaControl;

                                               foreach (var page in _tabPages)
                                               {
                                                   if (Convert.ToInt16(page.Value.Tag) == Convert.ToInt16(amc.Tag)+1)
                                                   {
                                                       xtraTabControl1.SelectedTabPage = page.Value;
                                                       writeRichEditControl.Focus();
                                                   }
                                               }
                                           }                                         
                                        };



                VideoPanels[i] = new PanelControl();
                VideoPanels[i].Controls.Add(AMCs[i]);
                AMCs[i].Controls.Add(NewVideoWindowLabelControl(String.Format("{0}",i+1)));
                //   VideoPanels[i].Visible = false;

            }
           // VideoWindowsLayoutPanel.Controls.AddRange(VideoPanels);
        }

        private void ShowVideoWindows ()
        {
            VideoWindowsLayoutPanel.SuspendLayout();

            VideoWindowsLayoutPanel.ColumnCount = 2;
            VideoWindowsLayoutPanel.RowCount = 2;

            VideoWindowsLayoutPanel.Controls.Clear();

        //    AMCInitialize();

            foreach (var panelControl in VideoPanels)
            {
                panelControl.Visible = false;
            }

            int videoWindowsCount = 0;

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            for (int i = 0; i < 4; i++)
            {
                if (Convert.ToBoolean(config.AppSettings.Settings[String.Format("EnabledVideoWindow{0}", i + 1)].Value))
                {
                    videoWindowsCount++;
                }
            }


            switch (videoWindowsCount)
            {
                case 1:
                    {
                        VideoWindowsLayoutPanel.ColumnCount = 1;
                        VideoWindowsLayoutPanel.RowCount = 1;
                    }
                    break;

                case 2:
                    {
                        VideoWindowsLayoutPanel.ColumnCount = 1;
                        VideoWindowsLayoutPanel.RowCount = 2;

                    }
                    break;

                case 3 - 4:
                    {
                        VideoWindowsLayoutPanel.ColumnCount = 2;
                        VideoWindowsLayoutPanel.RowCount = 2;
                    }
                    break;

                default:
                    {

                    }
                    break;

            }

             VideoWindowsLayoutPanel.Controls.AddRange(VideoPanels);
            
            
            
            for (int i = 0; i < 4; i++)
            {
                if (Convert.ToBoolean(config.AppSettings.Settings[String.Format("EnabledVideoWindow{0}", i + 1)].Value))
                {
                    VideoPanels[i].Visible = true;
                    VideoPanels[i].Dock = DockStyle.Fill;
                    AMCs[i].MediaURL = CompleteURL(Properties.Settings.Default[string.Format("VideoURL{0}", i + 1)].ToString(), Properties.Settings.Default.VideoType);
                    AMCs[i].MediaType = Properties.Settings.Default.VideoType;
                    AMCs[i].MediaUsername = Properties.Settings.Default.VideoUser;
                    AMCs[i].MediaPassword = Properties.Settings.Default.VideoPass;
                    AMCs[i].StretchToFit = true;
                    //   AMCs[i].ShowStatusBar = true;
                    AMCs[i].MaintainAspectRatio = true;
                    AMCs[i].BackgroundColor = 2960685;
                    AMCs[i].EnableReconnect = true;
                    AMCs[i].SetReconnectionStrategy(300000, 20000, 1800000, 60000, 10800000, 300000, true);
                    AMCs[i].Dock = DockStyle.Fill;
                    AMCs[i].Play();
                }
            
            }

            VideoWindowsLayoutPanel.ResumeLayout();

        }

        private void XtraForm1_Shown(object sender, EventArgs e)
        {
            // Получения конфигурации програмы
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Преобразование ID клиентов в int[] для передачи в конструктор серверной части
            int[] enabledIDs = Array.ConvertAll(config.AppSettings.Settings["EnabledClientIDs"].Value.Split('|'), int.Parse);

            //// Создание объекта серверной части
            _cu = new CommunicationUnit(Properties.Settings.Default.ReadPort1, Properties.Settings.Default.ReadPort2, Properties.Settings.Default.ReadPort3,
             Properties.Settings.Default.ReadPort4, Properties.Settings.Default.WritePort, Properties.Settings.Default.ClientID, Properties.Settings.Default.PortsSpeed, enabledIDs);

            //  Присвает ID широковещательных сообщений ID клиента
            _broadcastId = _cu.ClietnId;

            // Подписывание на событие
            _cu.MessageRecived += new EventHandler<MessageRecivedEventArgs>(ComPortMessageRecived);
            _cu.FileRequestRecived += new EventHandler<FileRequestRecivedEventArgs>(ComPortFileRequestRecived);

            StatusBarInitialize();
            QuickCommandsInitialize();
            RichEditControlsInitialize(enabledIDs);
            ////
            AMCInitialize();
            ShowVideoWindows();
        }

        private string CompleteURL(string theMediaURL, string theMediaType)
        {
            string anURL = theMediaURL;
            if (!anURL.EndsWith("/")) anURL += "/";

            if (theMediaType == "mjpeg")
            {
                anURL += "axis-cgi/mjpg/video.cgi";
            }
            else if (theMediaType == "mpeg4")
            {
                anURL += "mpeg4/media.amp";
            }
            else if (theMediaType == "h264")
            {
                anURL += "axis-media/media.amp?videocodec=h264";
            }
            else if (theMediaType == "mpeg2-unicast")
            {
                anURL += "axis-cgi/mpeg2/video.cgi";
            }
            else if (theMediaType == "mpeg2-multicast")
            {
                anURL += "axis-cgi/mpeg2/video.cgi";
            }

            anURL = CompleteProtocol(anURL, theMediaType);
            return anURL;
        }

        private string CompleteProtocol(string theMediaURL, string theMediaType)
        {
            if (theMediaURL.IndexOf("://") >= 0) return theMediaURL;

            string anURL = theMediaURL;

            if (theMediaType == "mjpeg")
            {
                anURL = "http://" + anURL;
            }
            else if (theMediaType == "mpeg4" || theMediaType == "h264")
            {
                anURL = "axrtsphttp://" + anURL;
            }
            else if (theMediaType == "mpeg2-unicast")
            {
                anURL = "http://" + anURL;
            }
            else if (theMediaType == "mpeg2-multicast")
            {
                anURL = "axsdp" + anURL;
            }

            return anURL;
        }


    }
}