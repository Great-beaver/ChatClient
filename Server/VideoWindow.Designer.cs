namespace Server
{
    partial class VideoWindow
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
            this.SuspendLayout();
            // 
            // VideoWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(590, 439);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "VideoWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "VideoWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VideoWindow_FormClosing);
            this.Shown += new System.EventHandler(this.VideoWindow_Shown);
            this.SizeChanged += new System.EventHandler(this.VideoWindow_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VideoWindow_KeyDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.VideoWindow_MouseUp);
            this.Move += new System.EventHandler(this.VideoWindow_Move);
            this.Resize += new System.EventHandler(this.VideoWindow_Resize);
            this.ResumeLayout(false);

        }

        #endregion
    }
}