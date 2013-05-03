namespace ChatClient
{
    partial class Form1
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
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.FileBut = new System.Windows.Forms.Button();
            this.writeRichTextBox = new System.Windows.Forms.RichTextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.CanBut = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.AllowBut = new System.Windows.Forms.Button();
            this.DenyBut = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.StatusLabelReadPort = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabelReadPortValue = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabelWritePort = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabelWritePortValue = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabelID = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabelIDValue = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.FileLabel = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Location = new System.Drawing.Point(284, 350);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(51, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Send";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 401);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(72, 20);
            this.textBox1.TabIndex = 1;
            this.textBox1.Text = "COM10";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(293, 398);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(41, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Ok";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(100, 401);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(72, 20);
            this.textBox2.TabIndex = 4;
            this.textBox2.Text = "COM11";
            // 
            // FileBut
            // 
            this.FileBut.Location = new System.Drawing.Point(227, 350);
            this.FileBut.Name = "FileBut";
            this.FileBut.Size = new System.Drawing.Size(51, 23);
            this.FileBut.TabIndex = 5;
            this.FileBut.Text = "File";
            this.FileBut.UseVisualStyleBackColor = true;
            this.FileBut.Click += new System.EventHandler(this.FileBut_Click);
            // 
            // writeRichTextBox
            // 
            this.writeRichTextBox.Location = new System.Drawing.Point(12, 234);
            this.writeRichTextBox.Name = "writeRichTextBox";
            this.writeRichTextBox.Size = new System.Drawing.Size(322, 110);
            this.writeRichTextBox.TabIndex = 6;
            this.writeRichTextBox.Text = "";
            this.writeRichTextBox.TextChanged += new System.EventHandler(this.writeRichTextBox_TextChanged);
            this.writeRichTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.writeRichTextBox_KeyDown);
            this.writeRichTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.writeRichTextBox_KeyPress);
            this.writeRichTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.writeRichTextBox_KeyUp);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // CanBut
            // 
            this.CanBut.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CanBut.Location = new System.Drawing.Point(228, 350);
            this.CanBut.Name = "CanBut";
            this.CanBut.Size = new System.Drawing.Size(50, 23);
            this.CanBut.TabIndex = 7;
            this.CanBut.Text = "Cancel";
            this.CanBut.UseVisualStyleBackColor = true;
            this.CanBut.Visible = false;
            this.CanBut.Click += new System.EventHandler(this.CanBut_Click);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(182, 401);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(35, 20);
            this.textBox3.TabIndex = 9;
            this.textBox3.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 385);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Read port";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(97, 385);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Write port";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(179, 385);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Id";
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(223, 401);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(64, 20);
            this.textBox5.TabIndex = 15;
            this.textBox5.Text = "38400";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(220, 385);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Port speed";
            // 
            // AllowBut
            // 
            this.AllowBut.Location = new System.Drawing.Point(110, 350);
            this.AllowBut.Name = "AllowBut";
            this.AllowBut.Size = new System.Drawing.Size(51, 23);
            this.AllowBut.TabIndex = 17;
            this.AllowBut.Text = "Allow";
            this.AllowBut.UseVisualStyleBackColor = true;
            this.AllowBut.Visible = false;
            this.AllowBut.Click += new System.EventHandler(this.AllowBut_Click);
            // 
            // DenyBut
            // 
            this.DenyBut.Location = new System.Drawing.Point(167, 350);
            this.DenyBut.Name = "DenyBut";
            this.DenyBut.Size = new System.Drawing.Size(51, 23);
            this.DenyBut.TabIndex = 18;
            this.DenyBut.Text = "Deny";
            this.DenyBut.UseVisualStyleBackColor = true;
            this.DenyBut.Visible = false;
            this.DenyBut.Click += new System.EventHandler(this.DenyBut_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(322, 216);
            this.tabControl1.TabIndex = 19;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabelReadPort,
            this.StatusLabelReadPortValue,
            this.StatusLabelWritePort,
            this.StatusLabelWritePortValue,
            this.StatusLabelID,
            this.StatusLabelIDValue});
            this.statusStrip1.Location = new System.Drawing.Point(0, 422);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
            this.statusStrip1.Size = new System.Drawing.Size(340, 22);
            this.statusStrip1.TabIndex = 20;
            this.statusStrip1.Text = "statusStrip1";
            this.statusStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.statusStrip1_ItemClicked);
            // 
            // StatusLabelReadPort
            // 
            this.StatusLabelReadPort.Name = "StatusLabelReadPort";
            this.StatusLabelReadPort.Size = new System.Drawing.Size(58, 17);
            this.StatusLabelReadPort.Text = "Read Port";
            // 
            // StatusLabelReadPortValue
            // 
            this.StatusLabelReadPortValue.Name = "StatusLabelReadPortValue";
            this.StatusLabelReadPortValue.Size = new System.Drawing.Size(0, 17);
            // 
            // StatusLabelWritePort
            // 
            this.StatusLabelWritePort.ForeColor = System.Drawing.SystemColors.ControlText;
            this.StatusLabelWritePort.Name = "StatusLabelWritePort";
            this.StatusLabelWritePort.Size = new System.Drawing.Size(57, 17);
            this.StatusLabelWritePort.Text = "WritePort";
            // 
            // StatusLabelWritePortValue
            // 
            this.StatusLabelWritePortValue.Name = "StatusLabelWritePortValue";
            this.StatusLabelWritePortValue.Size = new System.Drawing.Size(0, 17);
            // 
            // StatusLabelID
            // 
            this.StatusLabelID.Name = "StatusLabelID";
            this.StatusLabelID.Size = new System.Drawing.Size(18, 17);
            this.StatusLabelID.Text = "ID";
            // 
            // StatusLabelIDValue
            // 
            this.StatusLabelIDValue.Name = "StatusLabelIDValue";
            this.StatusLabelIDValue.Size = new System.Drawing.Size(0, 17);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(10, 350);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(207, 23);
            this.progressBar1.TabIndex = 21;
            this.progressBar1.Visible = false;
            // 
            // FileLabel
            // 
            this.FileLabel.AutoSize = true;
            this.FileLabel.Location = new System.Drawing.Point(12, 355);
            this.FileLabel.Name = "FileLabel";
            this.FileLabel.Size = new System.Drawing.Size(85, 13);
            this.FileLabel.TabIndex = 22;
            this.FileLabel.Text = "Принять файл?";
            this.FileLabel.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(340, 444);
            this.Controls.Add(this.FileLabel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.DenyBut);
            this.Controls.Add(this.AllowBut);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.CanBut);
            this.Controls.Add(this.writeRichTextBox);
            this.Controls.Add(this.FileBut);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button FileBut;
        private System.Windows.Forms.RichTextBox writeRichTextBox;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button CanBut;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button AllowBut;
        private System.Windows.Forms.Button DenyBut;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabelReadPort;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabelWritePort;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabelID;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabelReadPortValue;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabelWritePortValue;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabelIDValue;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label FileLabel;
    }
}

