namespace Server
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
            this.FileBut = new System.Windows.Forms.Button();
            this.writeRichTextBox = new System.Windows.Forms.RichTextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.CanBut = new System.Windows.Forms.Button();
            this.AllowBut = new System.Windows.Forms.Button();
            this.DenyBut = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.FileLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Location = new System.Drawing.Point(536, 350);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(51, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Send";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FileBut
            // 
            this.FileBut.Location = new System.Drawing.Point(479, 350);
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
            this.writeRichTextBox.Size = new System.Drawing.Size(575, 110);
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
            this.CanBut.Location = new System.Drawing.Point(480, 350);
            this.CanBut.Name = "CanBut";
            this.CanBut.Size = new System.Drawing.Size(50, 23);
            this.CanBut.TabIndex = 7;
            this.CanBut.Text = "Cancel";
            this.CanBut.UseVisualStyleBackColor = true;
            this.CanBut.Visible = false;
            this.CanBut.Click += new System.EventHandler(this.CanBut_Click);
            // 
            // AllowBut
            // 
            this.AllowBut.Location = new System.Drawing.Point(366, 350);
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
            this.DenyBut.Location = new System.Drawing.Point(423, 350);
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
            this.tabControl1.Size = new System.Drawing.Size(575, 216);
            this.tabControl1.TabIndex = 19;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 383);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
            this.statusStrip1.Size = new System.Drawing.Size(599, 22);
            this.statusStrip1.TabIndex = 20;
            this.statusStrip1.Text = "statusStrip1";
            this.statusStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.statusStrip1_ItemClicked);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(13, 350);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(461, 23);
            this.progressBar1.TabIndex = 21;
            this.progressBar1.Visible = false;
            // 
            // FileLabel
            // 
            this.FileLabel.AutoSize = true;
            this.FileLabel.Location = new System.Drawing.Point(268, 355);
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
            this.ClientSize = new System.Drawing.Size(599, 405);
            this.Controls.Add(this.FileLabel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.DenyBut);
            this.Controls.Add(this.AllowBut);
            this.Controls.Add(this.CanBut);
            this.Controls.Add(this.writeRichTextBox);
            this.Controls.Add(this.FileBut);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button FileBut;
        private System.Windows.Forms.RichTextBox writeRichTextBox;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button CanBut;
        private System.Windows.Forms.Button AllowBut;
        private System.Windows.Forms.Button DenyBut;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label FileLabel;
    }
}

