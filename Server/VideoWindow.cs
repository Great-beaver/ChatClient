using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AxAXISMEDIACONTROLLib;
using DevExpress.XtraEditors;

namespace Server
{
    public partial class VideoWindow : DevExpress.XtraEditors.XtraForm
    {
        public VideoWindow(VideoWindowInfo videoWindowInfo)
        {
            InitializeComponent();

            _videoWindowInfo = videoWindowInfo;

            this.ControlBox = false;

            this.Top = _videoWindowInfo.Top;
            this.Left = _videoWindowInfo.Left;
            this.Width = _videoWindowInfo.Width;
            this.Height = _videoWindowInfo.Height;

            AMC = new AxAxisMediaControl();
            AMC.Tag = videoWindowInfo.Number;
            AMC.OnDoubleClick += (s, ea) =>
            {
                if (s is AxAxisMediaControl)
                {
                    var amc = s as AxAxisMediaControl;
                    amc.FullScreen = !amc.FullScreen;
                }
            };
            Name = videoWindowInfo.Name;
            this.Text = WindowName;
        }

        private AxAxisMediaControl AMC;
        private VideoWindowInfo _videoWindowInfo;
        private bool _altF4Pressed = false;

        public string WindowName;

        private void VideoWindow_Shown(object sender, EventArgs e)
        {     
            this.Controls.Add(AMC);
            AMC.MediaURL = _videoWindowInfo.MediaUrl;
            AMC.MediaType = _videoWindowInfo.MediaType;
            AMC.MediaUsername = _videoWindowInfo.MediaUsername;
            AMC.MediaPassword = _videoWindowInfo.MediaPassword;
            AMC.StretchToFit = true;
            AMC.MaintainAspectRatio = true;
            AMC.BackgroundColor = 2960685;
            AMC.EnableReconnect = true;
            AMC.SetReconnectionStrategy(300000, 20000, 1800000, 60000, 10800000, 300000, true);
            AMC.Dock = DockStyle.Fill;
            AMC.Play();
        }

        private void VideoWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.F4)
                _altF4Pressed = true;
        }

        private void VideoWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_altF4Pressed)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                    e.Cancel = true;
                _altF4Pressed = false;
            }
        }

        private void VideoWindow_Move(object sender, EventArgs e)
        {   
        }

        private void VideoWindow_Resize(object sender, EventArgs e)
        {
        }

        private void VideoWindow_MouseUp(object sender, MouseEventArgs e)
        {
        }

        private void VideoWindow_SizeChanged(object sender, EventArgs e)
        {
        }
    }
}