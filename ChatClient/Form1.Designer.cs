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
            this.button3 = new System.Windows.Forms.Button();
            this.writeRichTextBox = new System.Windows.Forms.RichTextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
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
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(273, 60);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Send text";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(13, 288);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(72, 20);
            this.textBox1.TabIndex = 1;
            this.textBox1.Text = "COM10";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(300, 286);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(56, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Ok";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(101, 288);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(72, 20);
            this.textBox2.TabIndex = 4;
            this.textBox2.Text = "COM11";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(273, 89);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "Send file";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // writeRichTextBox
            // 
            this.writeRichTextBox.Location = new System.Drawing.Point(12, 159);
            this.writeRichTextBox.Name = "writeRichTextBox";
            this.writeRichTextBox.Size = new System.Drawing.Size(253, 110);
            this.writeRichTextBox.TabIndex = 6;
            this.writeRichTextBox.Text = "";
            // 
            // timer1
            // 
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(273, 118);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(95, 23);
            this.button4.TabIndex = 7;
            this.button4.Text = "Cancel s file";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(273, 147);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(95, 23);
            this.button5.TabIndex = 8;
            this.button5.Text = "Cancel r file";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(183, 288);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(35, 20);
            this.textBox3.TabIndex = 9;
            this.textBox3.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 272);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Read port";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(98, 272);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Write port";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(180, 272);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Id";
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(273, 34);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(75, 20);
            this.textBox4.TabIndex = 13;
            this.textBox4.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(270, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "To client";
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(224, 288);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(70, 20);
            this.textBox5.TabIndex = 15;
            this.textBox5.Text = "38400";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(221, 272);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Port speed";
            // 
            // AllowBut
            // 
            this.AllowBut.Location = new System.Drawing.Point(270, 187);
            this.AllowBut.Name = "AllowBut";
            this.AllowBut.Size = new System.Drawing.Size(75, 23);
            this.AllowBut.TabIndex = 17;
            this.AllowBut.Text = "Allow";
            this.AllowBut.UseVisualStyleBackColor = true;
            this.AllowBut.Visible = false;
            this.AllowBut.Click += new System.EventHandler(this.AllowBut_Click);
            // 
            // DenyBut
            // 
            this.DenyBut.Location = new System.Drawing.Point(270, 216);
            this.DenyBut.Name = "DenyBut";
            this.DenyBut.Size = new System.Drawing.Size(75, 23);
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
            this.tabControl1.Size = new System.Drawing.Size(255, 141);
            this.tabControl1.TabIndex = 19;
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
            this.statusStrip1.Location = new System.Drawing.Point(0, 314);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
            this.statusStrip1.Size = new System.Drawing.Size(383, 22);
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(383, 336);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.DenyBut);
            this.Controls.Add(this.AllowBut);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.writeRichTextBox);
            this.Controls.Add(this.button3);
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
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.RichTextBox writeRichTextBox;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Label label4;
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
        private System.Windows.Forms.Timer timer2;
    }
}

