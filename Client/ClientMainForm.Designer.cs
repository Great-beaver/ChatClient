namespace Client
{
    partial class ClientMainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClientMainForm));
            this.defaultLookAndFeel1 = new DevExpress.LookAndFeel.DefaultLookAndFeel(this.components);
            this.xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            this.progressBarControl1 = new DevExpress.XtraEditors.ProgressBarControl();
            this.SendBut = new DevExpress.XtraEditors.SimpleButton();
            this.FileBut = new DevExpress.XtraEditors.SimpleButton();
            this.CancelBut = new DevExpress.XtraEditors.SimpleButton();
            this.FileRequestLabel = new DevExpress.XtraEditors.LabelControl();
            this.AllowBut = new DevExpress.XtraEditors.SimpleButton();
            this.DenyBut = new DevExpress.XtraEditors.SimpleButton();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.writeRichEditControl = new DevExpress.XtraRichEdit.RichEditControl();
            this.barStaticItem2 = new DevExpress.XtraBars.BarStaticItem();
            this.barStaticItem1 = new DevExpress.XtraBars.BarStaticItem();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.IdValueLabelControl = new DevExpress.XtraEditors.LabelControl();
            this.IdLabelControl = new DevExpress.XtraEditors.LabelControl();
            this.WritePortValueLabelControl = new DevExpress.XtraEditors.LabelControl();
            this.WritePortLabelControl = new DevExpress.XtraEditors.LabelControl();
            this.ReadPortValueLabelControl = new DevExpress.XtraEditors.LabelControl();
            this.ReadPortLabelControl = new DevExpress.XtraEditors.LabelControl();
            this.ButMin = new DevExpress.XtraEditors.SimpleButton();
            this.ButClose = new DevExpress.XtraEditors.SimpleButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panelControl2 = new DevExpress.XtraEditors.PanelControl();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
            this.panelControl2.SuspendLayout();
            this.SuspendLayout();
            // 
            // defaultLookAndFeel1
            // 
            this.defaultLookAndFeel1.LookAndFeel.SkinName = "Darkroom";
            // 
            // xtraTabControl1
            // 
            this.xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xtraTabControl1.Location = new System.Drawing.Point(3, 3);
            this.xtraTabControl1.Name = "xtraTabControl1";
            this.xtraTabControl1.Size = new System.Drawing.Size(698, 270);
            this.xtraTabControl1.TabIndex = 0;
            this.xtraTabControl1.SelectedPageChanged += new DevExpress.XtraTab.TabPageChangedEventHandler(this.xtraTabControl1_SelectedPageChanged);
            this.xtraTabControl1.Click += new System.EventHandler(this.xtraTabControl1_Click);
            // 
            // progressBarControl1
            // 
            this.progressBarControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarControl1.Location = new System.Drawing.Point(5, 143);
            this.progressBarControl1.Name = "progressBarControl1";
            this.progressBarControl1.Size = new System.Drawing.Size(507, 33);
            this.progressBarControl1.TabIndex = 6;
            this.progressBarControl1.Visible = false;
            // 
            // SendBut
            // 
            this.SendBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SendBut.Appearance.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.SendBut.Appearance.Options.UseFont = true;
            this.SendBut.Location = new System.Drawing.Point(607, 143);
            this.SendBut.Name = "SendBut";
            this.SendBut.Size = new System.Drawing.Size(91, 33);
            this.SendBut.TabIndex = 7;
            this.SendBut.Text = "Отправить";
            this.SendBut.Click += new System.EventHandler(this.SendBut_Click);
            // 
            // FileBut
            // 
            this.FileBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FileBut.Appearance.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.FileBut.Appearance.Options.UseFont = true;
            this.FileBut.Location = new System.Drawing.Point(518, 143);
            this.FileBut.Name = "FileBut";
            this.FileBut.Size = new System.Drawing.Size(83, 33);
            this.FileBut.TabIndex = 8;
            this.FileBut.Text = "Файл";
            this.FileBut.Click += new System.EventHandler(this.FileBut_Click);
            // 
            // CancelBut
            // 
            this.CancelBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelBut.Appearance.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.CancelBut.Appearance.Options.UseFont = true;
            this.CancelBut.Location = new System.Drawing.Point(518, 143);
            this.CancelBut.Name = "CancelBut";
            this.CancelBut.Size = new System.Drawing.Size(83, 33);
            this.CancelBut.TabIndex = 9;
            this.CancelBut.Text = "Отмена";
            this.CancelBut.Visible = false;
            this.CancelBut.Click += new System.EventHandler(this.CancelBut_Click);
            // 
            // FileRequestLabel
            // 
            this.FileRequestLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FileRequestLabel.Appearance.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.FileRequestLabel.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.FileRequestLabel.Location = new System.Drawing.Point(136, 151);
            this.FileRequestLabel.Name = "FileRequestLabel";
            this.FileRequestLabel.Size = new System.Drawing.Size(101, 16);
            this.FileRequestLabel.TabIndex = 10;
            this.FileRequestLabel.Text = "Принять файл?";
            this.FileRequestLabel.Visible = false;
            // 
            // AllowBut
            // 
            this.AllowBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AllowBut.Appearance.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.AllowBut.Appearance.Options.UseFont = true;
            this.AllowBut.Location = new System.Drawing.Point(428, 143);
            this.AllowBut.Name = "AllowBut";
            this.AllowBut.Size = new System.Drawing.Size(84, 33);
            this.AllowBut.TabIndex = 15;
            this.AllowBut.Text = "Принять";
            this.AllowBut.Visible = false;
            this.AllowBut.Click += new System.EventHandler(this.AllowBut_Click);
            // 
            // DenyBut
            // 
            this.DenyBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DenyBut.Appearance.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.DenyBut.Appearance.Options.UseFont = true;
            this.DenyBut.Location = new System.Drawing.Point(297, 143);
            this.DenyBut.Name = "DenyBut";
            this.DenyBut.Size = new System.Drawing.Size(125, 33);
            this.DenyBut.TabIndex = 16;
            this.DenyBut.Text = "Не принимать";
            this.DenyBut.Visible = false;
            this.DenyBut.Click += new System.EventHandler(this.DenyBut_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // writeRichEditControl
            // 
            this.writeRichEditControl.ActiveViewType = DevExpress.XtraRichEdit.RichEditViewType.Simple;
            this.writeRichEditControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.writeRichEditControl.Location = new System.Drawing.Point(5, 29);
            this.writeRichEditControl.Name = "writeRichEditControl";
            this.writeRichEditControl.Options.Fields.UseCurrentCultureDateTimeFormat = false;
            this.writeRichEditControl.Options.HorizontalRuler.Visibility = DevExpress.XtraRichEdit.RichEditRulerVisibility.Hidden;
            this.writeRichEditControl.Options.HorizontalScrollbar.Visibility = DevExpress.XtraRichEdit.RichEditScrollbarVisibility.Hidden;
            this.writeRichEditControl.Options.Hyperlinks.ModifierKeys = System.Windows.Forms.Keys.None;
            this.writeRichEditControl.Options.MailMerge.KeepLastParagraph = false;
            this.writeRichEditControl.Options.VerticalRuler.Visibility = DevExpress.XtraRichEdit.RichEditRulerVisibility.Hidden;
            this.writeRichEditControl.Size = new System.Drawing.Size(693, 108);
            this.writeRichEditControl.TabIndex = 21;
            this.writeRichEditControl.Views.SimpleView.Padding = new System.Windows.Forms.Padding(5, 4, 4, 0);
            this.writeRichEditControl.PopupMenuShowing += new DevExpress.XtraRichEdit.PopupMenuShowingEventHandler(this.richEditControl1_PopupMenuShowing);
            this.writeRichEditControl.Click += new System.EventHandler(this.writeRichEditControl_Click);
            this.writeRichEditControl.DragDrop += new System.Windows.Forms.DragEventHandler(this.writeRichEditControl_DragDrop);
            this.writeRichEditControl.DragEnter += new System.Windows.Forms.DragEventHandler(this.writeRichEditControl_DragEnter);
            this.writeRichEditControl.DragOver += new System.Windows.Forms.DragEventHandler(this.writeRichEditControl_DragDrop);
            this.writeRichEditControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.writeRichEditControl_KeyDown);
            this.writeRichEditControl.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.writeRichEditControl_KeyPress);
            this.writeRichEditControl.KeyUp += new System.Windows.Forms.KeyEventHandler(this.writeRichEditControl_KeyUp);
            // 
            // barStaticItem2
            // 
            this.barStaticItem2.Caption = "barStaticItem2";
            this.barStaticItem2.Id = 11;
            this.barStaticItem2.Name = "barStaticItem2";
            this.barStaticItem2.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // barStaticItem1
            // 
            this.barStaticItem1.Caption = "barStaticItem1";
            this.barStaticItem1.Id = 8;
            this.barStaticItem1.Name = "barStaticItem1";
            this.barStaticItem1.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // panelControl1
            // 
            this.panelControl1.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.panelControl1.Appearance.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.panelControl1.Appearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.panelControl1.Appearance.Options.UseBackColor = true;
            this.panelControl1.Appearance.Options.UseBorderColor = true;
            this.panelControl1.Controls.Add(this.IdValueLabelControl);
            this.panelControl1.Controls.Add(this.IdLabelControl);
            this.panelControl1.Controls.Add(this.WritePortValueLabelControl);
            this.panelControl1.Controls.Add(this.WritePortLabelControl);
            this.panelControl1.Controls.Add(this.ReadPortValueLabelControl);
            this.panelControl1.Controls.Add(this.ReadPortLabelControl);
            this.panelControl1.Location = new System.Drawing.Point(2, 2);
            this.panelControl1.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Office2003;
            this.panelControl1.LookAndFeel.UseDefaultLookAndFeel = false;
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(540, 23);
            this.panelControl1.TabIndex = 31;
            this.panelControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.panelControl1_Paint);
            // 
            // IdValueLabelControl
            // 
            this.IdValueLabelControl.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.IdValueLabelControl.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.IdValueLabelControl.Dock = System.Windows.Forms.DockStyle.Left;
            this.IdValueLabelControl.Location = new System.Drawing.Point(343, 2);
            this.IdValueLabelControl.Name = "IdValueLabelControl";
            this.IdValueLabelControl.Size = new System.Drawing.Size(68, 19);
            this.IdValueLabelControl.TabIndex = 5;
            this.IdValueLabelControl.Text = "Id Value";
            // 
            // IdLabelControl
            // 
            this.IdLabelControl.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.IdLabelControl.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.IdLabelControl.Dock = System.Windows.Forms.DockStyle.Left;
            this.IdLabelControl.Location = new System.Drawing.Point(313, 2);
            this.IdLabelControl.Name = "IdLabelControl";
            this.IdLabelControl.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.IdLabelControl.Size = new System.Drawing.Size(30, 19);
            this.IdLabelControl.TabIndex = 4;
            this.IdLabelControl.Text = "ID:";
            // 
            // WritePortValueLabelControl
            // 
            this.WritePortValueLabelControl.Appearance.Font = new System.Drawing.Font("Tahoma", 12.25F, System.Drawing.FontStyle.Bold);
            this.WritePortValueLabelControl.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.WritePortValueLabelControl.Dock = System.Windows.Forms.DockStyle.Left;
            this.WritePortValueLabelControl.Location = new System.Drawing.Point(235, 2);
            this.WritePortValueLabelControl.Name = "WritePortValueLabelControl";
            this.WritePortValueLabelControl.Size = new System.Drawing.Size(78, 19);
            this.WritePortValueLabelControl.TabIndex = 3;
            this.WritePortValueLabelControl.Text = "WP Value";
            // 
            // WritePortLabelControl
            // 
            this.WritePortLabelControl.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.WritePortLabelControl.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.WritePortLabelControl.Dock = System.Windows.Forms.DockStyle.Left;
            this.WritePortLabelControl.Location = new System.Drawing.Point(141, 2);
            this.WritePortLabelControl.Name = "WritePortLabelControl";
            this.WritePortLabelControl.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.WritePortLabelControl.Size = new System.Drawing.Size(94, 19);
            this.WritePortLabelControl.TabIndex = 2;
            this.WritePortLabelControl.Text = "Передача:";
            // 
            // ReadPortValueLabelControl
            // 
            this.ReadPortValueLabelControl.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.ReadPortValueLabelControl.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.ReadPortValueLabelControl.Dock = System.Windows.Forms.DockStyle.Left;
            this.ReadPortValueLabelControl.Location = new System.Drawing.Point(67, 2);
            this.ReadPortValueLabelControl.Name = "ReadPortValueLabelControl";
            this.ReadPortValueLabelControl.Size = new System.Drawing.Size(74, 19);
            this.ReadPortValueLabelControl.TabIndex = 1;
            this.ReadPortValueLabelControl.Text = "RP Value";
            // 
            // ReadPortLabelControl
            // 
            this.ReadPortLabelControl.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
            this.ReadPortLabelControl.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.ReadPortLabelControl.Dock = System.Windows.Forms.DockStyle.Left;
            this.ReadPortLabelControl.Location = new System.Drawing.Point(2, 2);
            this.ReadPortLabelControl.Name = "ReadPortLabelControl";
            this.ReadPortLabelControl.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.ReadPortLabelControl.Size = new System.Drawing.Size(65, 19);
            this.ReadPortLabelControl.TabIndex = 0;
            this.ReadPortLabelControl.Text = "Прием:";
            // 
            // ButMin
            // 
            this.ButMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButMin.Appearance.Font = new System.Drawing.Font("Tahoma", 13F, System.Drawing.FontStyle.Bold);
            this.ButMin.Appearance.Options.UseFont = true;
            this.ButMin.Location = new System.Drawing.Point(620, -3);
            this.ButMin.Name = "ButMin";
            this.ButMin.Size = new System.Drawing.Size(39, 30);
            this.ButMin.TabIndex = 6;
            this.ButMin.Text = "-";
            this.ButMin.Click += new System.EventHandler(this.ButMin_Click);
            // 
            // ButClose
            // 
            this.ButClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButClose.Appearance.Font = new System.Drawing.Font("Tahoma", 13F, System.Drawing.FontStyle.Bold);
            this.ButClose.Appearance.Options.UseFont = true;
            this.ButClose.Location = new System.Drawing.Point(662, -3);
            this.ButClose.Name = "ButClose";
            this.ButClose.Size = new System.Drawing.Size(39, 30);
            this.ButClose.TabIndex = 1;
            this.ButClose.Text = "X";
            this.ButClose.Click += new System.EventHandler(this.ButClose_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.xtraTabControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panelControl2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1408, 276);
            this.tableLayoutPanel1.TabIndex = 31;
            // 
            // panelControl2
            // 
            this.panelControl2.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.panelControl2.Appearance.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.panelControl2.Appearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.panelControl2.Appearance.Options.UseBackColor = true;
            this.panelControl2.Appearance.Options.UseBorderColor = true;
            this.panelControl2.Controls.Add(this.flowLayoutPanel1);
            this.panelControl2.Controls.Add(this.ButMin);
            this.panelControl2.Controls.Add(this.panelControl1);
            this.panelControl2.Controls.Add(this.ButClose);
            this.panelControl2.Controls.Add(this.writeRichEditControl);
            this.panelControl2.Controls.Add(this.FileRequestLabel);
            this.panelControl2.Controls.Add(this.DenyBut);
            this.panelControl2.Controls.Add(this.SendBut);
            this.panelControl2.Controls.Add(this.AllowBut);
            this.panelControl2.Controls.Add(this.FileBut);
            this.panelControl2.Controls.Add(this.progressBarControl1);
            this.panelControl2.Controls.Add(this.CancelBut);
            this.panelControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl2.Location = new System.Drawing.Point(707, 3);
            this.panelControl2.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.Office2003;
            this.panelControl2.LookAndFeel.UseDefaultLookAndFeel = false;
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(698, 270);
            this.panelControl2.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Location = new System.Drawing.Point(5, 182);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(688, 84);
            this.flowLayoutPanel1.TabIndex = 42;
            this.flowLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.flowLayoutPanel1_Paint);
            // 
            // ClientMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1408, 276);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ClientMainForm";
            this.Text = "Mobile unit";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.XtraForm1_FormClosing);
            this.Load += new System.EventHandler(this.XtraForm1_Load);
            this.Resize += new System.EventHandler(this.XtraForm1_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
            this.panelControl2.ResumeLayout(false);
            this.panelControl2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.LookAndFeel.DefaultLookAndFeel defaultLookAndFeel1;
        private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
        private DevExpress.XtraEditors.ProgressBarControl progressBarControl1;
        private DevExpress.XtraEditors.LabelControl FileRequestLabel;
        private DevExpress.XtraEditors.SimpleButton CancelBut;
        private DevExpress.XtraEditors.SimpleButton FileBut;
        private DevExpress.XtraEditors.SimpleButton SendBut;
        private DevExpress.XtraEditors.SimpleButton DenyBut;
        private DevExpress.XtraEditors.SimpleButton AllowBut;
        private System.Windows.Forms.Timer timer1;
        private DevExpress.XtraRichEdit.RichEditControl writeRichEditControl;
        private DevExpress.XtraBars.BarStaticItem barStaticItem2;
        private DevExpress.XtraBars.BarStaticItem barStaticItem1;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.LabelControl ReadPortLabelControl;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private DevExpress.XtraEditors.LabelControl IdLabelControl;
        private DevExpress.XtraEditors.LabelControl WritePortValueLabelControl;
        private DevExpress.XtraEditors.LabelControl WritePortLabelControl;
        private DevExpress.XtraEditors.LabelControl ReadPortValueLabelControl;
        private DevExpress.XtraEditors.LabelControl IdValueLabelControl;
        private DevExpress.XtraEditors.PanelControl panelControl2;
        private DevExpress.XtraEditors.SimpleButton ButClose;
        private DevExpress.XtraEditors.SimpleButton ButMin;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}