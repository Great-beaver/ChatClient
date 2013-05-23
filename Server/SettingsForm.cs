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
        public SettingsForm(Boolean windowState1, Boolean windowState2, Boolean windowState3, Boolean windowState4)
        {
            InitializeComponent();
            WindowStateCheckEdit1.Checked = windowState1;
            WindowStateCheckEdit2.Checked = windowState2;
            WindowStateCheckEdit3.Checked = windowState3;
            WindowStateCheckEdit4.Checked = windowState4;
        }

        public bool WindowEnabled1 { get; private set; }
        public bool WindowEnabled2 { get; private set; }
        public bool WindowEnabled3 { get; private set; }
        public bool WindowEnabled4 { get; private set; }

        private void SettingsForm_Load(object sender, EventArgs e)
        {

        }

        private void SettingsForm_Shown(object sender, EventArgs e)
        {
            
        }
    }
}