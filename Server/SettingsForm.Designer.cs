namespace Server
{
    partial class SettingsForm
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
            this.OkButton = new DevExpress.XtraEditors.SimpleButton();
            this.CancelButton = new DevExpress.XtraEditors.SimpleButton();
            this.WindowStateCheckEdit1 = new DevExpress.XtraEditors.CheckEdit();
            this.settingBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.WindowStateCheckEdit2 = new DevExpress.XtraEditors.CheckEdit();
            this.WindowStateCheckEdit3 = new DevExpress.XtraEditors.CheckEdit();
            this.WindowStateCheckEdit4 = new DevExpress.XtraEditors.CheckEdit();
            this.defaultLookAndFeel1 = new DevExpress.LookAndFeel.DefaultLookAndFeel(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.WindowStateCheckEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.settingBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.WindowStateCheckEdit2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.WindowStateCheckEdit3.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.WindowStateCheckEdit4.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // OkButton
            // 
            this.OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OkButton.Location = new System.Drawing.Point(31, 134);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(75, 23);
            this.OkButton.TabIndex = 0;
            this.OkButton.Text = "Сохранить";
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(112, 134);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 1;
            this.CancelButton.Text = "Отмена";
            // 
            // WindowStateCheckEdit1
            // 
            this.WindowStateCheckEdit1.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.settingBindingSource, "WindowEnabled1", true));
            this.WindowStateCheckEdit1.Location = new System.Drawing.Point(34, 25);
            this.WindowStateCheckEdit1.Name = "WindowStateCheckEdit1";
            this.WindowStateCheckEdit1.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 13F, System.Drawing.FontStyle.Bold);
            this.WindowStateCheckEdit1.Properties.Appearance.Options.UseFont = true;
            this.WindowStateCheckEdit1.Properties.Caption = "Видео окно 1";
            this.WindowStateCheckEdit1.Size = new System.Drawing.Size(146, 26);
            this.WindowStateCheckEdit1.TabIndex = 2;
            // 
            // settingBindingSource
            // 
            this.settingBindingSource.DataSource = typeof(Server.Setting);
            // 
            // WindowStateCheckEdit2
            // 
            this.WindowStateCheckEdit2.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.settingBindingSource, "WindowEnabled2", true));
            this.WindowStateCheckEdit2.Location = new System.Drawing.Point(34, 52);
            this.WindowStateCheckEdit2.Name = "WindowStateCheckEdit2";
            this.WindowStateCheckEdit2.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 13F, System.Drawing.FontStyle.Bold);
            this.WindowStateCheckEdit2.Properties.Appearance.Options.UseFont = true;
            this.WindowStateCheckEdit2.Properties.Caption = "Видео окно 2";
            this.WindowStateCheckEdit2.Size = new System.Drawing.Size(146, 26);
            this.WindowStateCheckEdit2.TabIndex = 3;
            // 
            // WindowStateCheckEdit3
            // 
            this.WindowStateCheckEdit3.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.settingBindingSource, "WindowEnabled3", true));
            this.WindowStateCheckEdit3.Location = new System.Drawing.Point(34, 77);
            this.WindowStateCheckEdit3.Name = "WindowStateCheckEdit3";
            this.WindowStateCheckEdit3.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 13F, System.Drawing.FontStyle.Bold);
            this.WindowStateCheckEdit3.Properties.Appearance.Options.UseFont = true;
            this.WindowStateCheckEdit3.Properties.Caption = "Видео окно 3";
            this.WindowStateCheckEdit3.Size = new System.Drawing.Size(146, 26);
            this.WindowStateCheckEdit3.TabIndex = 4;
            // 
            // WindowStateCheckEdit4
            // 
            this.WindowStateCheckEdit4.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.settingBindingSource, "WindowEnabled4", true));
            this.WindowStateCheckEdit4.Location = new System.Drawing.Point(34, 102);
            this.WindowStateCheckEdit4.Name = "WindowStateCheckEdit4";
            this.WindowStateCheckEdit4.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 13F, System.Drawing.FontStyle.Bold);
            this.WindowStateCheckEdit4.Properties.Appearance.Options.UseFont = true;
            this.WindowStateCheckEdit4.Properties.Caption = "Видео окно 4";
            this.WindowStateCheckEdit4.Size = new System.Drawing.Size(146, 26);
            this.WindowStateCheckEdit4.TabIndex = 5;
            // 
            // defaultLookAndFeel1
            // 
            this.defaultLookAndFeel1.LookAndFeel.SkinName = "Darkroom";
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.OkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(217, 179);
            this.Controls.Add(this.WindowStateCheckEdit4);
            this.Controls.Add(this.WindowStateCheckEdit3);
            this.Controls.Add(this.WindowStateCheckEdit2);
            this.Controls.Add(this.WindowStateCheckEdit1);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OkButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SettingsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Настройки";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.Shown += new System.EventHandler(this.SettingsForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.WindowStateCheckEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.settingBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.WindowStateCheckEdit2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.WindowStateCheckEdit3.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.WindowStateCheckEdit4.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton OkButton;
        private DevExpress.XtraEditors.SimpleButton CancelButton;
        public DevExpress.XtraEditors.CheckEdit WindowStateCheckEdit1;
        public DevExpress.XtraEditors.CheckEdit WindowStateCheckEdit3;
        public DevExpress.XtraEditors.CheckEdit WindowStateCheckEdit4;
        public DevExpress.XtraEditors.CheckEdit WindowStateCheckEdit2;
        private DevExpress.LookAndFeel.DefaultLookAndFeel defaultLookAndFeel1;
        private System.Windows.Forms.BindingSource settingBindingSource;
    }
}