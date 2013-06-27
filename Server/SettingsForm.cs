using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace Server
{
   
    public partial class SettingsForm : DevExpress.XtraEditors.XtraForm
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        public SettingsForm(Setting setting) : this()
        {
            Setting = setting;
            settingBindingSource.DataSource = Setting;
        }

        public Setting Setting { get; private set; }

        private void SettingsForm_Load(object sender, EventArgs e)
        {

        }

        private void SettingsForm_Shown(object sender, EventArgs e)
        {
            
        }

        private void OkButton_Click(object sender, EventArgs e)
        {

        }

        private void WindowStateCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }

    public class Setting
    {
        public bool WindowEnabled1 { get; set; }
        public bool WindowEnabled2 { get; set; }
        public bool WindowEnabled3 { get; set; }
        public bool WindowEnabled4 { get; set; }
    }

}