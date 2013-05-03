namespace Client
{
    partial class XtraForm1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.defaultLookAndFeel1 = new DevExpress.LookAndFeel.DefaultLookAndFeel();
            this.xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            this.writeRichTextBox = new System.Windows.Forms.RichTextBox();
            this.barManager1 = new DevExpress.XtraBars.BarManager();
            this.bar3 = new DevExpress.XtraBars.Bar();
            this.barStaticReadPort = new DevExpress.XtraBars.BarStaticItem();
            this.barStaticReadPortValue = new DevExpress.XtraBars.BarStaticItem();
            this.barStaticWritePort = new DevExpress.XtraBars.BarStaticItem();
            this.barStaticWritePortValue = new DevExpress.XtraBars.BarStaticItem();
            this.barStaticId = new DevExpress.XtraBars.BarStaticItem();
            this.barStaticIdValue = new DevExpress.XtraBars.BarStaticItem();
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.progressBarControl1 = new DevExpress.XtraEditors.ProgressBarControl();
            this.SendBut = new DevExpress.XtraEditors.SimpleButton();
            this.FileBut = new DevExpress.XtraEditors.SimpleButton();
            this.CancelBut = new DevExpress.XtraEditors.SimpleButton();
            this.FileRequestLabel = new DevExpress.XtraEditors.LabelControl();
            this.AllowBut = new DevExpress.XtraEditors.SimpleButton();
            this.DenyBut = new DevExpress.XtraEditors.SimpleButton();
            this.timer1 = new System.Windows.Forms.Timer();
            this.richEditControl1 = new DevExpress.XtraRichEdit.RichEditControl();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // defaultLookAndFeel1
            // 
            this.defaultLookAndFeel1.LookAndFeel.SkinName = "Darkroom";
            // 
            // xtraTabControl1
            // 
            this.xtraTabControl1.Location = new System.Drawing.Point(12, 12);
            this.xtraTabControl1.Name = "xtraTabControl1";
            this.xtraTabControl1.Size = new System.Drawing.Size(346, 234);
            this.xtraTabControl1.TabIndex = 0;
            this.xtraTabControl1.SelectedPageChanged += new DevExpress.XtraTab.TabPageChangedEventHandler(this.xtraTabControl1_SelectedPageChanged);
            // 
            // writeRichTextBox
            // 
            this.writeRichTextBox.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.writeRichTextBox.Location = new System.Drawing.Point(13, 252);
            this.writeRichTextBox.Name = "writeRichTextBox";
            this.writeRichTextBox.Size = new System.Drawing.Size(345, 131);
            this.writeRichTextBox.TabIndex = 1;
            this.writeRichTextBox.Text = "";
            this.writeRichTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.writeRichTextBox_KeyDown);
            this.writeRichTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.writeRichTextBox_KeyUp);
            // 
            // barManager1
            // 
            this.barManager1.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.bar3});
            this.barManager1.DockControls.Add(this.barDockControlTop);
            this.barManager1.DockControls.Add(this.barDockControlBottom);
            this.barManager1.DockControls.Add(this.barDockControlLeft);
            this.barManager1.DockControls.Add(this.barDockControlRight);
            this.barManager1.Form = this;
            this.barManager1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.barStaticReadPort,
            this.barStaticReadPortValue,
            this.barStaticWritePort,
            this.barStaticWritePortValue,
            this.barStaticId,
            this.barStaticIdValue});
            this.barManager1.MaxItemId = 7;
            this.barManager1.StatusBar = this.bar3;
            // 
            // bar3
            // 
            this.bar3.BarName = "Status bar";
            this.bar3.CanDockStyle = DevExpress.XtraBars.BarCanDockStyle.Bottom;
            this.bar3.DockCol = 0;
            this.bar3.DockRow = 0;
            this.bar3.DockStyle = DevExpress.XtraBars.BarDockStyle.Bottom;
            this.bar3.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.barStaticReadPort),
            new DevExpress.XtraBars.LinkPersistInfo(this.barStaticReadPortValue),
            new DevExpress.XtraBars.LinkPersistInfo(this.barStaticWritePort),
            new DevExpress.XtraBars.LinkPersistInfo(this.barStaticWritePortValue),
            new DevExpress.XtraBars.LinkPersistInfo(this.barStaticId),
            new DevExpress.XtraBars.LinkPersistInfo(this.barStaticIdValue)});
            this.bar3.OptionsBar.AllowQuickCustomization = false;
            this.bar3.OptionsBar.DrawDragBorder = false;
            this.bar3.OptionsBar.UseWholeRow = true;
            this.bar3.Text = "Status bar";
            // 
            // barStaticReadPort
            // 
            this.barStaticReadPort.Caption = "Read Port";
            this.barStaticReadPort.Id = 1;
            this.barStaticReadPort.Name = "barStaticReadPort";
            this.barStaticReadPort.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // barStaticReadPortValue
            // 
            this.barStaticReadPortValue.Caption = "RP value";
            this.barStaticReadPortValue.Id = 2;
            this.barStaticReadPortValue.Name = "barStaticReadPortValue";
            this.barStaticReadPortValue.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // barStaticWritePort
            // 
            this.barStaticWritePort.Caption = "Write Port";
            this.barStaticWritePort.Id = 3;
            this.barStaticWritePort.Name = "barStaticWritePort";
            this.barStaticWritePort.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // barStaticWritePortValue
            // 
            this.barStaticWritePortValue.Caption = "WP value";
            this.barStaticWritePortValue.Id = 4;
            this.barStaticWritePortValue.Name = "barStaticWritePortValue";
            this.barStaticWritePortValue.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // barStaticId
            // 
            this.barStaticId.Caption = "ID";
            this.barStaticId.Id = 5;
            this.barStaticId.Name = "barStaticId";
            this.barStaticId.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // barStaticIdValue
            // 
            this.barStaticIdValue.Caption = "Id value";
            this.barStaticIdValue.Id = 6;
            this.barStaticIdValue.Name = "barStaticIdValue";
            this.barStaticIdValue.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Size = new System.Drawing.Size(504, 0);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 695);
            this.barDockControlBottom.Size = new System.Drawing.Size(504, 26);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 0);
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 695);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(504, 0);
            this.barDockControlRight.Size = new System.Drawing.Size(0, 695);
            // 
            // progressBarControl1
            // 
            this.progressBarControl1.Location = new System.Drawing.Point(13, 394);
            this.progressBarControl1.MenuManager = this.barManager1;
            this.progressBarControl1.Name = "progressBarControl1";
            this.progressBarControl1.Size = new System.Drawing.Size(186, 18);
            this.progressBarControl1.TabIndex = 6;
            this.progressBarControl1.Visible = false;
            // 
            // SendBut
            // 
            this.SendBut.Location = new System.Drawing.Point(286, 394);
            this.SendBut.Name = "SendBut";
            this.SendBut.Size = new System.Drawing.Size(75, 23);
            this.SendBut.TabIndex = 7;
            this.SendBut.Text = "Send";
            this.SendBut.Click += new System.EventHandler(this.SendBut_Click);
            // 
            // FileBut
            // 
            this.FileBut.Location = new System.Drawing.Point(205, 394);
            this.FileBut.Name = "FileBut";
            this.FileBut.Size = new System.Drawing.Size(75, 23);
            this.FileBut.TabIndex = 8;
            this.FileBut.Text = "File";
            this.FileBut.Click += new System.EventHandler(this.FileBut_Click);
            // 
            // CancelBut
            // 
            this.CancelBut.Location = new System.Drawing.Point(205, 394);
            this.CancelBut.Name = "CancelBut";
            this.CancelBut.Size = new System.Drawing.Size(75, 23);
            this.CancelBut.TabIndex = 9;
            this.CancelBut.Text = "Cancel";
            this.CancelBut.Visible = false;
            this.CancelBut.Click += new System.EventHandler(this.CancelBut_Click);
            // 
            // FileRequestLabel
            // 
            this.FileRequestLabel.Location = new System.Drawing.Point(13, 399);
            this.FileRequestLabel.Name = "FileRequestLabel";
            this.FileRequestLabel.Size = new System.Drawing.Size(77, 13);
            this.FileRequestLabel.TabIndex = 10;
            this.FileRequestLabel.Text = "Принять файл?";
            this.FileRequestLabel.Visible = false;
            // 
            // AllowBut
            // 
            this.AllowBut.Location = new System.Drawing.Point(12, 418);
            this.AllowBut.Name = "AllowBut";
            this.AllowBut.Size = new System.Drawing.Size(75, 23);
            this.AllowBut.TabIndex = 15;
            this.AllowBut.Text = "Allow";
            this.AllowBut.Visible = false;
            this.AllowBut.Click += new System.EventHandler(this.AllowBut_Click);
            // 
            // DenyBut
            // 
            this.DenyBut.Location = new System.Drawing.Point(93, 418);
            this.DenyBut.Name = "DenyBut";
            this.DenyBut.Size = new System.Drawing.Size(75, 23);
            this.DenyBut.TabIndex = 16;
            this.DenyBut.Text = "Deny";
            this.DenyBut.Visible = false;
            this.DenyBut.Click += new System.EventHandler(this.DenyBut_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // richEditControl1
            // 
            this.richEditControl1.ActiveViewType = DevExpress.XtraRichEdit.RichEditViewType.Simple;
            this.richEditControl1.Location = new System.Drawing.Point(13, 447);
            this.richEditControl1.MenuManager = this.barManager1;
            this.richEditControl1.Name = "richEditControl1";
            this.richEditControl1.Options.Fields.UseCurrentCultureDateTimeFormat = false;
            this.richEditControl1.Options.HorizontalRuler.Visibility = DevExpress.XtraRichEdit.RichEditRulerVisibility.Hidden;
            this.richEditControl1.Options.HorizontalScrollbar.Visibility = DevExpress.XtraRichEdit.RichEditScrollbarVisibility.Hidden;
            this.richEditControl1.Options.MailMerge.KeepLastParagraph = false;
            this.richEditControl1.Options.VerticalRuler.Visibility = DevExpress.XtraRichEdit.RichEditRulerVisibility.Hidden;
            this.richEditControl1.Size = new System.Drawing.Size(400, 200);
            this.richEditControl1.TabIndex = 21;
            this.richEditControl1.Text = "richEditControl1";
            this.richEditControl1.Views.SimpleView.Padding = new System.Windows.Forms.Padding(5, 4, 4, 0);
            // 
            // XtraForm1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 721);
            this.Controls.Add(this.richEditControl1);
            this.Controls.Add(this.DenyBut);
            this.Controls.Add(this.AllowBut);
            this.Controls.Add(this.FileRequestLabel);
            this.Controls.Add(this.FileBut);
            this.Controls.Add(this.SendBut);
            this.Controls.Add(this.progressBarControl1);
            this.Controls.Add(this.writeRichTextBox);
            this.Controls.Add(this.xtraTabControl1);
            this.Controls.Add(this.CancelBut);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "XtraForm1";
            this.Text = "Mobile unit";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.XtraForm1_FormClosing);
            this.Load += new System.EventHandler(this.XtraForm1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl1.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.LookAndFeel.DefaultLookAndFeel defaultLookAndFeel1;
        private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
        private System.Windows.Forms.RichTextBox writeRichTextBox;
        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar3;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraEditors.ProgressBarControl progressBarControl1;
        private DevExpress.XtraEditors.LabelControl FileRequestLabel;
        private DevExpress.XtraEditors.SimpleButton CancelBut;
        private DevExpress.XtraEditors.SimpleButton FileBut;
        private DevExpress.XtraEditors.SimpleButton SendBut;
        private DevExpress.XtraBars.BarStaticItem barStaticReadPort;
        private DevExpress.XtraBars.BarStaticItem barStaticReadPortValue;
        private DevExpress.XtraBars.BarStaticItem barStaticWritePort;
        private DevExpress.XtraBars.BarStaticItem barStaticWritePortValue;
        private DevExpress.XtraBars.BarStaticItem barStaticId;
        private DevExpress.XtraBars.BarStaticItem barStaticIdValue;
        private DevExpress.XtraEditors.SimpleButton DenyBut;
        private DevExpress.XtraEditors.SimpleButton AllowBut;
        private System.Windows.Forms.Timer timer1;
        private DevExpress.XtraRichEdit.RichEditControl richEditControl1;
    }
}